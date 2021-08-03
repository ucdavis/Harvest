import React, { useContext } from "react";
import { Link } from "react-router-dom";

import AppContext from "../Shared/AppContext";

export const HomeContainer = () => {
  const userInfo = useContext(AppContext);

  return (
    <div className="row mt-3">
      <div className="col">
        <h1>Welcome to Harvest, {userInfo.user.detail.name}</h1>
        <p>
          You have the following roles: {userInfo.user.roles.join(", ")}. Lorem
          ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
          tempor incididunt ut labore et dolore magna aliqua.
        </p>
        <hr />
        <h5>Quick Actions</h5>
        <ul className="list-group quick-actions">
          <li className="list-group-item">
            <Link to="/project">Cras justo odio</Link>
          </li>
          <li className="list-group-item">
            <Link to="/project">Dapibus ac facilisis in</Link>
          </li>
          <li className="list-group-item">
            <Link to="/project">Morbi leo risus</Link>
          </li>
          <li className="list-group-item">
            <Link to="/project">Porta ac consectetur ac</Link>
          </li>
          <li className="list-group-item">
            <Link to="/project">Vestibulum at ero</Link>
          </li>
        </ul>
      </div>
      <div className="col text-center">
        <img
          className="img-fluid"
          src="/media/studentfarmer.svg"
          alt="Pencil Sketch of farmer holding produce"
        ></img>
      </div>
    </div>
  );
};
