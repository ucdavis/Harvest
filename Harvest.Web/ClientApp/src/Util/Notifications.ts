import { useState } from "react";
import toast from "react-hot-toast";
import { PromiseStatus } from "../types";
import { isFunction } from "./TypeChecks";

// just re-export the whole module so we don't take direct dependencies all over
export * from "react-hot-toast";

// given a fetch, will return a promise that resolves with the fetch's response but fails on a non 200 response
export const fetchWithFailOnNotOk = (fetchPromise: Promise<any>) => {
  return new Promise((resolve, reject) => {
    fetchPromise.then((response) => {
      if (response.ok) {
        resolve(response);
      } else {
        reject(response);
      }
    });
  });
};

export const genericErrorMessage: string =
  "Something went wrong, please try again";

// returns notification object and notification setter
// call notification setter to initiate a loading notification
export const usePromiseNotification = (): [
  PromiseStatus,
  (
    promise: Promise<any>,
    loadingMessage: string,
    successMessage: string | (() => string)
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

      toast.promise(fetchWithFailOnNotOk(promise), {
        loading: loadingMessage,
        success: () => {
          setSuccess(true);
          setPending(false);
          if (isFunction(successMessage)) {
            return successMessage();
          }
          return successMessage;
        },
        error: () => {
          setSuccess(false);
          setPending(false);
          return errorMessage;
        },
      });
    },
  ];
};
