import React, { useState } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardBody, CardHeader, Alert } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMobile, faExternalLinkAlt } from "@fortawesome/free-solid-svg-icons";
import toast from "react-hot-toast";
import { CommonRouteParams } from "../types";
import { authenticatedFetch } from "../Util/Api";

export const MobileTokenContainer = () => {
  const { team } = useParams<CommonRouteParams>();
  const [mobileToken, setMobileToken] = useState<string>("");
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>("");

  const generateMobileToken = async () => {
    setIsLoading(true);
    setError("");

    try {
      console.log("Generating mobile token for team:", team);
      const response = await authenticatedFetch(`/api/${team}/Link`);

      if (response.ok) {
        const token = await response.json();
        setMobileToken(token);
        toast.success("Mobile token generated successfully!");
      } else {
        const errorText = await response.text();
        setError(errorText || "Failed to generate mobile token");
        toast.error("Failed to generate mobile token");
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "An unexpected error occurred";
      setError(errorMessage);
      toast.error("Error generating mobile token");
    } finally {
      setIsLoading(false);
    }
  };

  const openMobileApp = () => {
    if (mobileToken) {
      const baseUrl = window.location.origin;
      const appLink = `harvestmobile://applink?code=${mobileToken}&baseUrl=${baseUrl}`;
      window.location.href = appLink;
      toast.success("Opening mobile app...");
    }
  };

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-8">
          <Card>
            <CardHeader>
              <h4>
                <FontAwesomeIcon icon={faMobile} className="me-2" />
                Mobile Token Generator
              </h4>
            </CardHeader>
            <CardBody>
              <p className="text-muted mb-4">
                Generate a mobile token to authenticate our app.
              </p>

              <div className="mb-4">
                <Button
                  color="primary"
                  onClick={generateMobileToken}
                  disabled={isLoading}
                  size="lg"
                >
                  {isLoading ? "Generating..." : "Generate Mobile Token"}
                </Button>
              </div>

              {error && (
                <Alert color="danger" className="mb-4">
                  <strong>Error:</strong> {error}
                </Alert>
              )}

              {mobileToken && (
                <div className="mt-4">
                  <h5>Mobile Token:</h5>
                  <div className="d-flex align-items-center">
                    <div
                      className="form-control me-2"
                      style={{
                        fontFamily: "monospace",
                        fontSize: "0.9rem",
                        wordBreak: "break-all",
                        minHeight: "60px",
                        display: "flex",
                        alignItems: "center",
                      }}
                    >
                      {mobileToken}
                    </div>
                    <Button
                      color="primary"
                      onClick={openMobileApp}
                      title="Authorize Mobile App"
                      size="sm"
                    >
                      <FontAwesomeIcon
                        icon={faExternalLinkAlt}
                        className="me-2"
                      />
                      Authorize Mobile App
                    </Button>
                  </div>
                  <small className="text-muted mt-2 d-block">
                    Keep this token secure and use it within 5 minutes to
                    authenticate our mobile app. Otherwise you will need to
                    regenerate this.
                  </small>
                </div>
              )}
            </CardBody>
          </Card>
        </div>
      </div>
    </div>
  );
};
