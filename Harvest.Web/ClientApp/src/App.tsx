import React from "react";

import { Layout } from "./Layout";
import { Route } from "react-router-dom";

function App() {
  return (
    <Layout>
      <Route exact path="/" component={Home} />
      <Route exact path="/projects" component={Projects} />
    </Layout>
  );
}

const Home = () => <div>Home</div>;
const Projects = () => <div>Projects</div>;

export default App;
