import React from "react";
import ReactDOM from "react-dom";
import App from "./App";
import reportWebVitals from "./reportWebVitals";
import { BrowserRouter } from "react-router-dom";

import "react-bootstrap-typeahead/css/Typeahead.css";
import "react-datepicker/dist/react-datepicker.css";
import "./sass/harvest.scss";

// Import MVC helpers to make them globally available
import "./Shared/MvcHelpers/RateQRHelper";

const rootElement = document.getElementById("root");

if (rootElement) {
  // <React.StrictMode> should be used when possible.  ReactStrap will need to update context API usage first
  ReactDOM.render(
    <BrowserRouter>
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
