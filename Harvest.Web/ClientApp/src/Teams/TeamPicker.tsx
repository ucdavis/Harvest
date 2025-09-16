import { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck } from "@fortawesome/free-solid-svg-icons";

import { useHistory, useLocation } from "react-router-dom";
import { Team } from "../types";
import LoadingHarvest from "../Shared/loadingHarvest";

// Allow selection of a team from a list of teams, then add that team slug to url and redirect
export const TeamPicker = () => {
  // get the current page location
  const location = useLocation();

  const history = useHistory();

  // store teams in state
  const [teams, setTeams] = useState<Team[]>();

  // get the teams from the server
  useEffect(() => {
    const getTeams = async () => {
      const response = await fetch("/api/Team");
      const data = await response.json();

      // if there is only one team, redirect to that team's page immediately
      if (data.length === 1) {
        history.replace(`/${data[0].slug}${location.pathname}`);
      } else {
        setTeams(data);
      }
    };
    getTeams();
  }, [history, location.pathname]);

  // show loading message while we wait for teams
  if (!teams) {
    return (
      <>
        {" "}
        <div className="p-4 text-center">
          <LoadingHarvest size={64} />
          {/* default color #266041 */}
          <p>Loading Teams...</p>
        </div>
      </>
    );
  }

  // show the list of teams in big boxes
  return (
    <div>
      <h2>Choose Team</h2>
      <hr />
      <div className="row">
        {teams.map((team) => (
          <div className="col-md-6 team-card" key={team.id}>
            <a href={`/${team.slug}${location.pathname}`}>
              <div className="card">
                <div className="card-body d-flex flex-column">
                  <h5 className="card-title">{team.name}</h5>
                  <p className="secondary-font">
                    Field Managers: {team.fieldManagers}
                  </p>
                  <p className="primary-font">{team.description}</p>
                  <button className="mt-auto w-16 btn btn-primary">
                    <b>
                      <FontAwesomeIcon icon={faCheck} /> Use this team
                    </b>
                  </button>
                </div>
              </div>
            </a>
          </div>
        ))}
      </div>
    </div>
  );
};
