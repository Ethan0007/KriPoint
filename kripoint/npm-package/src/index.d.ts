import type { AxiosInstance, CreateAxiosDefaults } from "axios";

export interface KriPointPayload {
  payload: string;
  iv: string;
}

export interface KriPointInterceptorOptions {
  key: string;
  decryptResponse?: boolean;
  excludePaths?: string[];
  onEncryptError?: (error: Error) => void;
  onDecryptError?: (error: Error) => void;
}

export interface KriPointDetachHandle {
  detach(): void;
}

export declare function attachKriPointInterceptor(
  axiosInstance: AxiosInstance,
  options: KriPointInterceptorOptions
): KriPointDetachHandle;

export declare function createKriPointAxios(
  axiosConfig?: CreateAxiosDefaults,
  interceptorOptions?: KriPointInterceptorOptions
): Promise<AxiosInstance>;

export declare function encrypt(value: unknown, base64Key: string): Promise<KriPointPayload>;
export declare function decrypt(payload: string, iv: string, base64Key: string): Promise<unknown>;
export declare function generateBase64Key(): Promise<string>;
export declare function isValidKey(base64Key: string): boolean;
