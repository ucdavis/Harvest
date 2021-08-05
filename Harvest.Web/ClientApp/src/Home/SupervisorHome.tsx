import React, { useContext } from "react";
import { Link } from "react-router-dom";

import AppContext from "../Shared/AppContext";

export const SupervisorHome = () => {
  const userInfo = useContext(AppContext);

  return (
    <>
      <h5>Quick Actions</h5>
      <ul className="list-group quick-actions">
        <li className="list-group-item">
          <Link to="/project">View Projects</Link>
        </li>
        <li className="list-group-item">
          <Link to="/expense/entry">Enter Expenses</Link>
        </li>
      </ul>
    </>
  );
};
