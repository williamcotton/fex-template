class HTTPError extends Error {
  constructor(statusCode, ...params) {
    super(...params);
    this.name = "HTTPError";
    this.statusCode = statusCode;
  }
}

const cacheKey = (url, options) =>
  `${url}-(${options ? JSON.stringify(options) : ""})`;

export default () => (req, res, next) => {
  req.fetchJson = async (url, options = {}, cacheOptions = {}) => {
    try {
      console.log(url, options);
      const response = await fetch(url, options);
      console.log(response);
      if (!response.ok) {
        throw new HTTPError(
          response.status,
          `HTTP error, status = ${response.status}`
        );
      }

      const data = await response.json(); // Assuming JSON response for simplicity

      // Generate a cache key and cache the response data unconditionally
      const key = cacheKey(url, options);
      res.cacheQuery(key, data);

      return data;
    } catch (error) {
      if (error instanceof HTTPError) {
        // Re-throw HTTPError instances to be handled by error-handling middleware
        throw error;
      }
      // Handle generic fetch errors
      throw new HTTPError(500, `Failed to fetch: ${error.message}`);
    }
  };
  next();
};
