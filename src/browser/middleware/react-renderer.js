import ReactDOM from "react-dom";
import { createRoot } from "react-dom/client";
import React from "react";
import serialize from "form-serialize";

export default ({ app, appLayout }) =>
  (req, res, next) => {
    const onClick = (e) => {
      e.preventDefault();
      function hrefOrParent(target) {
        if (target.href) {
          return target.href;
        }
        if (target.parentElement) {
          return hrefOrParent(target.parentElement);
        }
        return false;
      }
      const href = hrefOrParent(e.currentTarget);
      app.navigate(href);
    };

    const Link = (props) => {
      const mergedProps = { onClick, ...props };
      return React.createElement("a", mergedProps);
    };

    req.Link = Link;

    const Form = (props) => {
      const onSubmit = (e) => {
        e.preventDefault();
        const body = serialize(e.target, { hash: true, empty: true });
        const method = e.target.method;
        const action = props.baseAction
          ? req.baseUrl + props.baseAction
          : e.target.action;
        app.submit(action, method, body);
      };
      const mergedProps = { onSubmit, ...props };
      const { children } = mergedProps;
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
        method: "post",
        children: children, 
        style: { display: "inline" },
        ...props
      };

      // Create and return a Form component instance with the necessary props
      return React.createElement(Form, mergedProps);
    };

    req.FormButton = FormButton;

    res.renderComponent = (content, options = {}) => {
      const { title, description } = options;
      const statusCode = options.statusCode || 200;
      const layout = options.layout || appLayout;
      const { appContainer } = req.renderDocument({ title, description });
      const root = app.root || createRoot(appContainer);
      app.root = root;
      root.render(React.createElement(layout, { content, req }));
      res.status(statusCode);
      res.send();
    };

    res.renderErrorComponent = (content, options = {}) => {
      const { title, description } = options;
      const layout = options.layout || appLayout;
      const { appContainer } = req.renderDocument({ title, description });
      const root = app.root || createRoot(appContainer);
      app.root = root;
      root.render(React.createElement(layout, { content, req }));
      res.status(500);
    };

    next();
  };
