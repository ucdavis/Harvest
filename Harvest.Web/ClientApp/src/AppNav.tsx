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
import { useParams } from "react-router";
import { useLocation } from "react-router-dom";

const getTeam = (pathName: string) => {
  // return the first path token in the pathName
  if (!pathName) {
    return "";
  } else {
    const pathSegments = pathName.split("/");

    if (pathSegments.length > 1) {
      return pathSegments[1];
    } else {
      return "";
    }
  }
};

export const AppNav = () => {
  const user = useContext(AppContext).user;
  const [isOpen, setIsOpen] = useState(false);

  const location = useLocation();
  const team = getTeam(location.pathname);

  const toggle = () => setIsOpen(!isOpen);

  return (
    <div className="nav-wrapper">
      <div className="container">
        <Navbar color="light" light expand="md">
          <NavbarToggler onClick={toggle} />
          <Collapse className="justify-content-between" isOpen={isOpen} navbar>
            <Nav navbar>
              <ShowFor roles={["FieldManager", "Supervisor"]}>
                <NavItem>
                  <NavLink href={`/${team}/project`}>All Projects</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor roles={["PI"]}>
                <NavItem>
                  <NavLink href="/project/mine">My Projects</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor roles={["FieldManager", "Supervisor", "Worker"]}>
                <NavItem>
                  <NavLink href={`/${team}/expense/entry`}>Expenses</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor roles={["FieldManager", "Supervisor"]}>
                <UncontrolledDropdown nav inNavbar>
                  <DropdownToggle nav caret>
                    Admin
                  </DropdownToggle>
                  <DropdownMenu right>
                    <ShowFor roles={["FieldManager"]}>
                      <DropdownItem href={`/${team}/Permissions/Index`}>
                        Permissions
                      </DropdownItem>
                    </ShowFor>
                    <DropdownItem href={`/${team}/Rate/Index`}>
                      Rates
                    </DropdownItem>
                    <ShowFor roles={["FieldManager"]}>
                      <DropdownItem href={`/${team}/Crop/Index`}>
                        Crops
                      </DropdownItem>
                    </ShowFor>
                    <ShowFor roles={["FieldManager"]}>
                      <DropdownItem divider />
                      <DropdownItem href={`/${team}/project/adhocproject`}>
                        Ad-Hoc Project
                      </DropdownItem>
                    </ShowFor>
                    <DropdownItem divider />
                    <DropdownItem href={`/${team}/Project/Completed`}>
                      Completed Projects
                    </DropdownItem>
                    <DropdownItem href={`/${team}/Project/NeedsAttention`}>
                      Projects Needing Attention
                    </DropdownItem>
                    <DropdownItem href={`/${team}/Ticket/NeedsAttention`}>
                      Open Tickets
                    </DropdownItem>
                    <ShowFor roles={["System"]}>
                      <DropdownItem divider />
                      <DropdownItem href={`/System/UpdatePendingExpenses`}>
                        Unprocessed Expenses
                      </DropdownItem>
                      <DropdownItem href={`/System/Emulate`}>
                        Emulate
                      </DropdownItem>
                      <DropdownItem href={`/Report/AllProjects`}>
                        Reports - Projects
                      </DropdownItem>
                      <DropdownItem href={`/Report/HistoricalRateActivity`}>
                        Reports - Historical Rate Activity
                      </DropdownItem>
                    </ShowFor>
                  </DropdownMenu>
                </UncontrolledDropdown>
              </ShowFor>

              <NavItem>
                <NavLink href={`/Help`}>Help</NavLink>
              </NavItem>
            </Nav>
            <div className="d-flex align-items-center user-sign-in">
              Welcome, {user.detail.name}
              <form action="/Account/Logout" method="post" id="logoutForm">
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
