import { renderToString } from "react-dom/server";
import React from "react";

const Link = (props) => React.createElement("a", props);

const DefaultLayout = ({ content, req }) => {
  return React.createElement("div", { className: "sitewrapper" }, [
    React.createElement("div", { className: "content" }, [content]),
  ]);
};

export default ({ appLayout }) =>
  (req, res, next) => {
    req.Link = Link;

    const Form = (props) => {
      let mergedProps = { ...props };
      const { children } = mergedProps;
      mergedProps.action = props.baseAction
        ? req.baseUrl + props.baseAction
        : props.action;
      delete mergedProps.children;
      delete mergedProps.baseAction;
      delete mergedProps.buttonText;
      const formElements = [].concat(children);
      formElements.push(
        React.createElement("input", {
          key: "csrf",
          type: "hidden",
          name: "_csrf",
          value: req.csrf,
        })
      );
      return React.createElement("form", mergedProps, formElements);
    };

    req.Form = Form;

    const FormButton = (props) => {
      const { name, value, buttonText } = props;

      // Define the children (form elements) to be passed to the Form component
      const children = [
        React.createElement("input", {
          type: "hidden",
          name: name,
          value: value,
          key: "hiddenInput",
        }),
        React.createElement("input", {
          type: "submit",
          value: buttonText,
          key: "submitButton",
        }),
      ];

      const mergedProps = {
        action: props.baseAction ? req.baseUrl + props.baseAction : props.action,
        method: "post",
        children: children,
        style: { display: "inline" },
        ...props,
      };

      // Create and return a Form component instance with the necessary props
      return React.createElement(Form, mergedProps);
    }

    req.FormButton = FormButton;

    res.renderComponent = (content, options = {}) => {
      const layout = options.layout || appLayout || DefaultLayout;
      const renderedContent = renderToString(
        React.createElement(layout, { content, req })
      );
      const { title, description } = options;
      const statusCode = options.statusCode || 200;
      res.writeHead(statusCode, { "Content-Type": "text/html" });
      res.end(
        req.renderDocument({
          renderedContent,
          title,
          description,
        })
      );
    };

    res.renderErrorComponent = res.renderComponent;

    next();
  };
