import React from "react";

import { Layout } from "./Layout";
import { Route } from "react-router-dom";
import { QuoteContainer } from "./Quotes/QuoteContainer";

function App() {
  return (
    <Layout>
      <Route exact path="/" component={Home} />
      <Route path="/quote/create/:projectId" component={QuoteContainer} />
    </Layout>
  );
}

const Home = () => <div>Home</div>;

export default App;
