const localQueryCache = {};
let initialRequest = true;

class HTTPError extends Error {
  constructor(statusCode, ...params) {
    super(...params);
    this.name = "HTTPError";
    this.statusCode = statusCode;
  }
}

const fetchCacheKey = (url, options) =>
  `${url}-(${options ? JSON.stringify(options) : ""})`;

export default () => (req, res, next) => {
  req.fetchJson = async (url, options = {}, cacheOptions = {}) => {
    const cache = "cache" in cacheOptions ? cacheOptions.cache : true;
    const refresh = "refresh" in cacheOptions ? cacheOptions.refresh : false;
    const key = fetchCacheKey(url, options);

    // Check cache first
    const cachedResponse =
      initialRequest || (cache && !initialRequest) ? localQueryCache[key] : false;

    const fetchJson = async () => {
      const response = await fetch(url, {
        ...options,
        headers: {
          ...options.headers,
          "Content-Type": "application/json",
          Accept: "application/json",
        },
      });

      if (!response.ok) {
        throw new HTTPError(
          response.status,
          `HTTP error, status = ${response.status}`
        );
      }

      return response.json();
    };

    // Use cached response if available and not refreshing
    const response =
      cachedResponse && !refresh ? cachedResponse : await fetchJson();

    // Cache the new response if needed
    if (cache && !initialRequest) {
      localQueryCache[key] = response;
    }

    // No need to separate data and errors as we're not dealing with GraphQL specific responses
    return response;
  };
};