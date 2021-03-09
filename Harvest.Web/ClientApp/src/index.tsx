import React from "react";
import ReactDOM from "react-dom";
import App from "./App";
import reportWebVitals from "./reportWebVitals";
import { BrowserRouter } from "react-router-dom";

import "bootstrap/dist/css/bootstrap.css";

const rootElement = document.getElementById("root");

const baseUrl =
  document.getElementsByTagName("base")[0]?.getAttribute("href") || undefined;

if (rootElement && baseUrl) {
  // <React.StrictMode> should be used when possible.  ReactStrap will need to update context API usage first
  ReactDOM.render(
    <BrowserRouter basename={baseUrl}>
      <React.Fragment>
        <App />
      </React.Fragment>
    </BrowserRouter>,
    rootElement
  );

  // If you want to start measuring performance in your app, pass a function
  // to log results (for example: reportWebVitals(console.log))
  // or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
  reportWebVitals();
}
