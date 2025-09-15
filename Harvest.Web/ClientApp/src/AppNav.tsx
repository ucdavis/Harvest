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
import { useLocation } from "react-router-dom";

const getFirstPath = (pathName: string) => {
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
  const firstPath = getFirstPath(location.pathname).toLowerCase();

  const team = firstPath;

  const nonTeamPage =
    firstPath === "system" || firstPath === "project" || firstPath === "team";

  const toggle = () => setIsOpen(!isOpen);

  return (
    <div className="nav-wrapper px-0">
      <div className="container">
        <Navbar expand="md" className="px-0">
          <NavbarToggler onClick={toggle} />
          <Collapse className="justify-content-between" isOpen={isOpen} navbar>
            <Nav className="px-0" navbar>
              <ShowFor roles={["System"]} condition={nonTeamPage}>
                <NavItem>
                  <NavLink href="/">Home</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor roles={["PI"]}>
                <NavItem>
                  <NavLink href="/project/mine">My Projects</NavLink>
                </NavItem>
              </ShowFor>
              <ShowFor condition={!nonTeamPage}>
                <ShowFor roles={["FieldManager", "Supervisor", "Finance"]}>
                  <NavItem>
                    <NavLink href={`/${team}/project`}>All Projects</NavLink>
                  </NavItem>
                </ShowFor>
                <ShowFor roles={["Worker"]}>
                  <NavItem>
                    <NavLink href={`/${team}/mobile/token`}>App Link</NavLink>
                  </NavItem>
                </ShowFor>
                <ShowFor roles={["Worker"]}>
                  <NavItem>
                    <NavLink href={`/${team}/expense/entry`}>Expenses</NavLink>
                  </NavItem>
                </ShowFor>
                <ShowFor roles={["FieldManager", "Supervisor", "Finance"]}>
                  <UncontrolledDropdown nav inNavbar>
                    <DropdownToggle nav caret>
                      Team Admin
                    </DropdownToggle>
                    <DropdownMenu end>
                      <ShowFor roles={["FieldManager"]}>
                        <DropdownItem href={`/${team}/Permissions/Index`}>
                          Permissions
                        </DropdownItem>
                      </ShowFor>
                      <ShowFor roles={["Finance"]}>
                        <DropdownItem href={`/${team}/Rate/Index`}>
                          Rates
                        </DropdownItem>
                      </ShowFor>
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
                      <ShowFor roles={["FieldManager", "Supervisor"]}>
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
                        <DropdownItem divider />
                        <ShowFor roles={["Supervisor"]}>
                          <DropdownItem
                            href={`/${team}/Expense/GetMyPendingExpenses`}
                          >
                            My Worker's Pending Expenses
                          </DropdownItem>
                        </ShowFor>
                        <ShowFor roles={["FieldManager"]}>
                          <DropdownItem
                            href={`/${team}/Expense/GetAllPendingExpenses`}
                          >
                            All Pending Expenses
                          </DropdownItem>
                          <DropdownItem
                            href={`/${team}/Expense/GetApprovedExpenses`}
                          >
                            All Aprroved Expenses
                          </DropdownItem>
                        </ShowFor>
                      </ShowFor>
                      <ShowFor roles={["FieldManager", "Finance"]}>
                        <DropdownItem divider />
                        <DropdownItem href={`/${team}/Report/AllProjects`}>
                          Reports - Projects
                        </DropdownItem>
                        <DropdownItem
                          href={`/${team}/Report/HistoricalRateActivity`}
                        >
                          Reports - Historical Rate Activity
                        </DropdownItem>
                        <DropdownItem href={`/${team}/Report/StaleProjects`}>
                          Reports - Stale Projects
                        </DropdownItem>
                      </ShowFor>
                    </DropdownMenu>
                  </UncontrolledDropdown>
                </ShowFor>
              </ShowFor>
              <ShowFor roles={["System"]}>
                <UncontrolledDropdown nav inNavbar>
                  <DropdownToggle nav caret>
                    System Admin
                  </DropdownToggle>
                  <DropdownMenu end>
                    <DropdownItem href={`/System/UpdatePendingExpenses`}>
                      Unprocessed Expenses
                    </DropdownItem>
                    <DropdownItem href={`/System/Emulate`}>
                      Emulate
                    </DropdownItem>
                  </DropdownMenu>
                </UncontrolledDropdown>
              </ShowFor>

              <NavItem>
                <NavLink href={`/${team}/Help`}>Help</NavLink>
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
