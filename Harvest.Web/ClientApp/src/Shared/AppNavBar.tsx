import React, { useState } from "react";
import { ShowFor } from "../Shared/ShowFor";
import {
  Collapse,
  Navbar,
  NavbarToggler,
  NavbarBrand,
  Nav,
  NavItem,
  NavLink,
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
  NavbarText,
} from "reactstrap";

export const AppNavBar = () => {
  const [isOpen, setIsOpen] = useState(false);

  const toggle = () => setIsOpen(!isOpen);

  return (
    <div className="nav-wrapper">
      <div className="container">
        <Navbar color="light" light expand="md">
          <NavbarBrand href="/">Home</NavbarBrand>
          <NavbarToggler onClick={toggle} />
          <Collapse isOpen={isOpen} navbar>
            <Nav className="mr-auto" navbar>
              <NavItem>
                <NavLink href="/project">My Projects</NavLink>
              </NavItem>
              <ShowFor roles={["FieldManager", "Supervisor"]}>
                {" "}
                <NavItem>
                  <NavLink href="/project/mine">All Projects</NavLink>
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
            <NavbarText>Some Simple Text </NavbarText>
          </Collapse>
        </Navbar>
      </div>
    </div>
  );
};

// <div class="nav-wrapper">
//         <nav class="navbar navbar-expand-md nav container">
//             <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarCollapse"
//                 aria-controls="navbarCollapse" aria-expanded="false" aria-label="Toggle navigation">
//                 <span class="navbar-toggler-icon">Menu</span>
//             </button>
//             <div class="collapse navbar-collapse" id="navbarCollapse">
//                 <ul class="navbar-nav mr-auto">
//                     @if (await UserService.HasAccess(AccessCodes.SupervisorAccess))
//                     {
//                     <li class="nav-item active">
//                         <a class="nav-link" href="/project">Projects</a>
//                     </li>
//                     }
//                     else
//                     {
//                         <li class="nav-item active">
//                             <a class="nav-link" href="/project/mine">My Projects</a>
//                         </li>
//                     }

//                     @if (await UserService.HasAccess(AccessCodes.FieldManagerAccess))
//                     {
//                         <li class="nav-item">
//                             <a asp-controller="Permissions" asp-action="Index" class="nav-link">Permissions</a>
//                         </li>
//                         <li class="nav-item">
//                             <a asp-controller="Rate" asp-action="Index" class="nav-link">Rates</a>
//                         </li>
//                         <li class="nav-item">
//                             <a asp-controller="Crop" asp-action="Index" class="nav-link">Crops</a>
//                         </li>
//                     }
//                     @if (await UserService.HasAccess(AccessCodes.WorkerAccess))
//                     {
//                         <li class="nav-item">
//                             <a href="/expense/entry" class="nav-link">Expenses</a>
//                         </li>
//                     }
//                     <li class="nav-item">
//                         <a asp-controller="Help" asp-action="Index" class="nav-link">Help</a>
//                     </li>
//                 </ul>
//                 <div class="row align-items-center">
//                     Welcome @User.GetNameClaim()
//                     <form class="flexer" asp-area="" asp-controller="Account" asp-action="Logout" method="post" id="logoutForm">
//                         <button class="btn btn-link btn-sm" type="submit">Sign out</button>
//                     </form>
//                 </div>
//             </div>
//         </nav>

//         </div>
