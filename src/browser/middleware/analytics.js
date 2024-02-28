let initialRequest = true;

export default ({ analyticsRouter }) => {
  const analyticsPublish = async (type, req, res, params) => {
    console.log('analyticsPublish', type);
    const {
      url,
      method,
      headers: { referer },
    } = req;
    const { statusCode } = res;
    console.log('analyticsPublish', type, url, statusCode, method, params);
    const response = await fetch('/analytics', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
        'X-CSRF-Token': req.csrf,
        'Override-Referer': referer,
      },
      body: JSON.stringify({ type, url, statusCode, method, ...params }),
    });
    const json = await response.json();
    return json;
  };

  return (req, res, next) => {
    res.on('finish', async () => {
      // as this is also handled server-side, we don't want to track the initial request twice
      if (!initialRequest) {
        req.url = req.originalUrl;
        res.pageview = params => {
          analyticsPublish('pageview', req, res, params);
        };
        res.event = (params) => analyticsPublish('event', req, res, params);
        analyticsRouter(req, res, () => {
          console.log('analyticsPublish', 'log');
          analyticsPublish('log', req, res, {});
        });
      }
      initialRequest = false;
    });
    next();
  };
};
