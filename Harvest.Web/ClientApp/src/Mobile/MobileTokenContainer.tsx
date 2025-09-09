import React, { useState, useEffect } from "react";
import { Redirect, useParams } from "react-router-dom";
import { Button, Card, CardBody, CardHeader, Alert } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMobile } from "@fortawesome/free-solid-svg-icons";
import toast from "react-hot-toast";
import { CommonRouteParams } from "../types";
import { authenticatedFetch } from "../Util/Api";

export const MobileTokenContainer = () => {
  const { team: routeTeam } = useParams<CommonRouteParams>();
  const [team, setTeam] = useState<string | null>(routeTeam || null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isLoadingTeam, setIsLoadingTeam] = useState<boolean>(!routeTeam);
  const [error, setError] = useState<string>("");
  const [isMobileAppOpened, setIsMobileAppOpened] = useState<boolean>(false);
  const [isAuthorized, setIsAuthorized] = useState<boolean>(false);

  // Fetch team if not provided from route params
  useEffect(() => {
    const fetchTeam = async () => {
      if (!routeTeam) {
        setIsLoadingTeam(true);
        try {
          const response = await authenticatedFetch("/api/Link/GetTeam");
          if (response.ok) {
            const teamSlug = await response.json();
            if (teamSlug && teamSlug.trim()) {
              setTeam(teamSlug.trim());
            } else {
              // No team found, redirect to team selection
              setTeam(null);
            }
          } else {
            // API call failed, redirect to team selection
            setTeam(null);
          }
        } catch (err) {
          // Error occurred, redirect to team selection
          setTeam(null);
        } finally {
          setIsLoadingTeam(false);
        }
      }
    };

    fetchTeam();
  }, [routeTeam]);

  const generateMobileToken = async () => {
    setIsLoading(true);
    setError("");

    try {
      const response = await authenticatedFetch(`/api/${team}/Link`);

      if (response.ok) {
        const token = await response.json();

        // Automatically open mobile app with the token
        const baseUrl = window.location.origin;
        const appLink = `harvestmobile://applink?code=${token}&baseUrl=${encodeURIComponent(
          baseUrl
        )}`;
        window.location.href = appLink;

        // Mark as authorized and opened
        setIsMobileAppOpened(true);
        setIsAuthorized(true);
        toast.success("Mobile app authorized successfully!");
      } else {
        const errorText = await response.text();
        setError(errorText || "Failed to authorized mobile app");
        toast.error("Failed to authorized mobile app");
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "An unexpected error occurred";
      setError(errorMessage);
      toast.error("Error authorizing mobile app");
    } finally {
      setIsLoading(false);
    }
  };

  // Show loading state while fetching team
  if (isLoadingTeam) {
    return (
      <div className="container">
        <div className="row justify-content-center">
          <div className="col-md-8">
            <Card>
              <CardBody className="text-center">
                <p>Loading...</p>
              </CardBody>
            </Card>
          </div>
        </div>
      </div>
    );
  }

  // Redirect to team selection if no team is available
  if (!team) {
    return <Redirect to="/team" />;
  }

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-8">
          <Card>
            <CardHeader>
              <h4>
                <FontAwesomeIcon icon={faMobile} className="mr-2" />
                Mobile Token Generator
              </h4>
            </CardHeader>
            <CardBody>
              <p className="text-muted mb-4">
                Authorize Harvest Mobile App to use this team with your account.
              </p>

              {!isAuthorized && (
                <div className="mb-4">
                  <Button
                    color="primary"
                    onClick={generateMobileToken}
                    disabled={isLoading || isMobileAppOpened}
                    size="lg"
                  >
                    {isLoading ? "Authorizing..." : "Authorize Mobile App"}
                  </Button>
                </div>
              )}

              {error && (
                <Alert color="danger" className="mb-4">
                  <strong>Error:</strong> {error}
                </Alert>
              )}

              {isAuthorized && (
                <Alert color="success" className="mb-4">
                  <h5 className="mb-2">✅ Mobile App Authorized!</h5>
                  <p className="mb-0">
                    Your mobile app has been successfully authorized for this
                    team. You can now close this page and use the mobile app.
                  </p>
                </Alert>
              )}
            </CardBody>
          </Card>
        </div>
      </div>
    </div>
  );
};
