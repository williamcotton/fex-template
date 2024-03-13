/* global window, document */

import qs from "qs";

export default () => (req, res, next) => {
  const appContainer = document.querySelector("#app");
  var expressLink = JSON.parse(appContainer.dataset.expresslink);
  var defaultTitle = expressLink.defaultTitle;

  Object.keys(expressLink).forEach((key) => (req[key] = expressLink[key])); // eslint-disable-line no-return-assign

  req.renderDocument = ({ title }) => {
    document.querySelector("title").innerText = title || defaultTitle; // eslint-disable-line no-param-reassign
    return { appContainer: document.querySelector("#app") };
  };

  res.navigate = (path, query, replace) => {
    const pathname = query ? `${path}?${qs.stringify(query)}` : path;
    res.redirect(pathname, replace);
  };

  res.redirect = res.redirect.bind(res);

  res.redirectBackWithNewQuery = (query) => {
    res.navigate(req.baseUrl, query, true);
  };

  res.redirectBack = (query) => {
    const referrer = req.headers.referer;
    const referrerUrl = new URL(referrer);
    const referrerQuery = qs.parse(referrerUrl.search, { ignoreQueryPrefix: true });
    res.navigate(req.baseUrl, { ...referrerQuery, ...query }, true);
  };
  next();
};
