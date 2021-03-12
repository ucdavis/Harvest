import React from "react";

import { Route } from "react-router-dom";
import { QuoteContainer } from "./Quotes/QuoteContainer";

function App() {
  return (
    <>
      <Route exact path="/" component={Home} />
      <Route exact path="/home/spa" component={Spa} />
      <Route path="/quote/create/:projectId" component={QuoteContainer} />
    </>
  );
}

const Home = () => <div>Home</div>;
const Spa = () => <div>I am a SPA</div>;

export default App;
