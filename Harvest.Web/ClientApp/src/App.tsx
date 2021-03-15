import React from "react";

import { Route } from "react-router-dom";
import { QuoteContainer } from "./Quotes/QuoteContainer";
import { Map } from "./Maps/Map";

function App() {
  return (
    <>
      <Route exact path="/" component={Home} />
      <Route exact path="/home/spa" component={Spa} />
      <Route path="/quote/create/:projectId" component={QuoteContainer} />
      <Route path="/home/map" component={Map} />
    </>
  );
}

const Home = () => <div>Home</div>;
const Spa = () => <div className="sassy">I am a SPA</div>;

export default App;
