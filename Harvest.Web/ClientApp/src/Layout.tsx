import React from "react";
import { Container } from "reactstrap";
import { NavBar } from "./Nav";

export const Layout = (props: any) => (
  <div>
    <NavBar />
    <Container>{props.children}</Container>
  </div>
);
