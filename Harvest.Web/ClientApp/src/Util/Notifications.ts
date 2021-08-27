import { useState } from "react";
import toast from "react-hot-toast";
import { PromiseStatus } from "../types";
import { isPromise, isString } from "./TypeChecks";

// just re-export the whole module so we don't take direct dependencies all over
export * from "react-hot-toast";

// given a fetch, will return a promise that resolves with the fetch's response but fails on a non 200 response
export const fetchWithFailOnNotOk = (fetchPromise: Promise<any>) => {
  return new Promise<any>((resolve, reject) => {
    fetchPromise.then((response) => {
      if (response.ok) {
        resolve(response);
      } else {
        reject(response);
      }
    });
  });
};

export const genericErrorMessage: string = "Something went wrong, please try again";

// allows message to be static or determined by sync or async callback that takes result of promise
type MessageOrCallback = string | ((result: any) => string) | ((result: any) => Promise<string>);
const getMessage = async (value: MessageOrCallback, result: any) => {
  if (isString(value)) {
    return value;
  } else {
    const callbackValue = value(result);
    if (isPromise(callbackValue)) {
      return await callbackValue;
    }
    return callbackValue;
  }
}

// returns notification object and notification setter
// call notification setter to initiate a loading notification
export const usePromiseNotification = (): [
  PromiseStatus,
  (
    promise: Promise<any>,
    loadingMessage: string,
    successMessage: MessageOrCallback,
    errorMessage?: MessageOrCallback
  ) => void
] => {
  const [pending, setPending] = useState(false);
  const [success, setSuccess] = useState(false);

  return [
    {
      pending,
      success,
    } as PromiseStatus,
    (
      promise,
      loadingMessage,
      successMessage,
      errorMessage = genericErrorMessage
    ) => {
      setPending(true);

      // not using toast.promise() because it doesn't allow getting a message based on result of promise
      (async () => {
        let toastLoadingId: string | undefined;
        try {
          toastLoadingId = toast.loading(loadingMessage);
          const result = await fetchWithFailOnNotOk(promise);
          toast.success(await getMessage(successMessage, result));
          setSuccess(true);
          setPending(false);
        } catch (error) {
          toast.error(await getMessage(errorMessage, error));
          setSuccess(false);
          setPending(false);
        } finally {
          toast.dismiss(toastLoadingId);
        }
      })();
    },
  ];
};
