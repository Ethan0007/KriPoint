import { encrypt, decrypt } from "./crypto.js";

export function attachKriPointInterceptor(axiosInstance, options = {}) {
  const {
    key,
    decryptResponse = false,
    excludePaths = [],
    onEncryptError = null,
    onDecryptError = null,
  } = options;

  if (!key) {
    throw new Error("[KriPoint] attachKriPointInterceptor: `key` option is required.");
  }

  const requestId = axiosInstance.interceptors.request.use(
    async (config) => {
      const method = (config.method || "get").toLowerCase();
      const hasBody = ["post", "put", "patch", "delete"].includes(method);
      const isExcluded = excludePaths.some((p) => (config.url || "").includes(p));

      if (!hasBody || isExcluded || config.data === undefined || config.data === null) {
        return config;
      }

      try {
        const value =
          typeof config.data === "string" ? JSON.parse(config.data) : config.data;

        const encrypted = await encrypt(value, key);

        config.data = encrypted;
        config.headers["Content-Type"] = "application/json";
        config.headers["X-KriPoint"] = "1";
      } catch (err) {
        if (onEncryptError) {
          onEncryptError(err);
        } else {
          throw err;
        }
      }

      return config;
    },
    (error) => Promise.reject(error)
  );

  const responseId = axiosInstance.interceptors.response.use(
    async (response) => {
      if (!decryptResponse) return response;

      const data = response.data;
      if (data && typeof data.payload === "string" && typeof data.iv === "string") {
        try {
          response.data = await decrypt(data.payload, data.iv, key);
        } catch (err) {
          if (onDecryptError) onDecryptError(err);
        }
      }

      return response;
    },
    (error) => Promise.reject(error)
  );

  return {
    detach() {
      axiosInstance.interceptors.request.eject(requestId);
      axiosInstance.interceptors.response.eject(responseId);
    },
  };
}

export async function createKriPointAxios(axiosConfig = {}, interceptorOptions = {}) {
  const axios = (await import("axios")).default;
  const instance = axios.create(axiosConfig);
  attachKriPointInterceptor(instance, interceptorOptions);
  return instance;
}
