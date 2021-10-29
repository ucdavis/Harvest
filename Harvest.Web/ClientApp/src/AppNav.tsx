import React, { useContext, useState } from "react";
import { ShowFor } from "./Shared/ShowFor";
import {
  Collapse,
  Navbar,
  NavbarToggler,
  Nav,
  NavItem,
  NavLink,
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
} from "reactstrap";
import AppContext from "./Shared/AppContext";

export const AppNav = () => {
  const user = useContext(AppContext).user;
  const [isOpen, setIsOpen] = useState(false);

  const toggle = () => setIsOpen(!isOpen);

  return (
    <div className="nav-wrapper">
      <div className="container">
        <Navbar color="light" light expand="md">
          <NavbarToggler onClick={toggle} />
          <Collapse isOpen={isOpen} navbar>
            <Nav className="mr-auto" navbar>
              <ShowFor roles={["FieldManager", "Supervisor"]}>
                <NavItem>
                  <NavLink href="/project">All Projects</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor roles={["PI"]}>
                <NavItem>
                  <NavLink href="/project/mine">My Projects</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor roles={["FieldManager", "Supervisor", "Worker"]}>
                <NavItem>
                  <NavLink href="/expense/entry">Expenses</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor roles={["FieldManager", "Supervisor"]}>
                <UncontrolledDropdown nav inNavbar>
                  <DropdownToggle nav caret>
                    Admin
                  </DropdownToggle>
                  <DropdownMenu right>
                    <DropdownItem href="/Permissions/Index">
                      Permissions
                    </DropdownItem>
                    <DropdownItem href="/Rate/Index">Rates</DropdownItem>
                    <DropdownItem href="/Crop/Index">Crops</DropdownItem>
                  </DropdownMenu>
                </UncontrolledDropdown>
              </ShowFor>
            </Nav>
            <div className="row align-items-center">
              Welcome {user.detail.name}
              <form
                className="flexer"
                action="/Account/Logout"
                method="post"
                id="logoutForm"
              >
                <button className="btn btn-link btn-sm" type="submit">
                  Sign out
                </button>
              </form>
            </div>
          </Collapse>
        </Navbar>
      </div>
    </div>
  );
};
