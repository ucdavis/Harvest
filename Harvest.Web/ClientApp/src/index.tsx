import React from "react";
import ReactDOM from "react-dom";
import App from "./App";
import reportWebVitals from "./reportWebVitals";
import { BrowserRouter } from "react-router-dom";

import "bootstrap/dist/css/bootstrap.css";

const baseUrl =
  document.getElementsByTagName("base")[0]?.getAttribute("href") || undefined;

// NOTE: We are using baseURL here to determine if we should load react.  Another good option would be to check to see if our "root" element exists
if (baseUrl !== undefined) {
  // <React.StrictMode> should be used when possible.  ReactStrap will need to update context API usage first
  ReactDOM.render(
    <BrowserRouter basename={baseUrl}>
      <React.Fragment>
        <App />
      </React.Fragment>
    </BrowserRouter>,
    document.getElementById("root")
  );

  // If you want to start measuring performance in your app, pass a function
  // to log results (for example: reportWebVitals(console.log))
  // or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
  reportWebVitals();
}
