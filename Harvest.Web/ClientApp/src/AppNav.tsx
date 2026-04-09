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
import { RoleName } from "./types";

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
  const teamAdminRoles: RoleName[] = ["FieldManager", "Supervisor", "Finance"];
  const teamReportRoles: RoleName[] = ["FieldManager", "Supervisor", "Finance"];

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
                <ShowFor roles={teamAdminRoles}>
                  <NavItem>
                    <NavLink href={`/${team}/project`}>All Projects</NavLink>
                  </NavItem>
                </ShowFor>
                <ShowFor roles={["Worker"]}>
                  <NavItem>
                    <NavLink href={`/${team}/expense/entry`}>Expenses</NavLink>
                  </NavItem>
                </ShowFor>
                <ShowFor roles={teamAdminRoles}>
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
                        <ShowFor roles={["Supervisor", "FieldManager"]}>
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
                            All Approved Expenses
                          </DropdownItem>
                        </ShowFor>
                      </ShowFor>
                    </DropdownMenu>
                  </UncontrolledDropdown>
                </ShowFor>
                <ShowFor roles={teamReportRoles}>
                  <UncontrolledDropdown nav inNavbar>
                    <DropdownToggle nav caret>
                      Team Reports
                    </DropdownToggle>
                    <DropdownMenu end>
                      <ShowFor roles={["FieldManager", "Finance"]}>
                        <DropdownItem href={`/${team}/Report/AllProjects`}>
                          Projects
                        </DropdownItem>
                        <DropdownItem href={`/${team}/Report/StaleProjects`}>
                          Stale Projects
                        </DropdownItem>

                        <DropdownItem
                          href={`/${team}/Report/ProjectsUnbilledExpenses`}
                        >
                          Projects Unbilled Expenses
                        </DropdownItem>
                        <DropdownItem href={`/${team}/Report/UnbilledExpenses`}>
                          Unbilled Expenses
                        </DropdownItem>
                        <DropdownItem divider />
                        <DropdownItem
                          href={`/${team}/Report/HistoricalRateActivity`}
                        >
                          Historical Rate Activity
                        </DropdownItem>
                      </ShowFor>
                      <ShowFor roles={["FieldManager", "Supervisor"]}>
                        <ShowFor roles={["FieldManager", "Finance"]}>
                          <DropdownItem divider />
                        </ShowFor>
                        <DropdownItem href={`/${team}/Report/WeeklyHoursByWorker`}>
                          Weekly Hours by Worker
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
