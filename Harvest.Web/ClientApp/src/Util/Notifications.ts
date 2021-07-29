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

export const genericError: string = "Something went wrong, please try again";
