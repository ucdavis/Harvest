import React, { useState } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardBody, CardHeader, Alert } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMobile, faCopy } from "@fortawesome/free-solid-svg-icons";
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
      const response = await authenticatedFetch(`/api/${team}/Link`);

      if (response.ok) {
        const token = await response.text();
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

  const copyToClipboard = async () => {
    if (mobileToken) {
      try {
        await navigator.clipboard.writeText(mobileToken);
        toast.success("Mobile token copied to clipboard!");
      } catch (err) {
        toast.error("Failed to copy to clipboard");
      }
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
                Generate a mobile token for API access. This token can be used
                to authenticate mobile applications.
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
                      color="secondary"
                      onClick={copyToClipboard}
                      title="Copy to clipboard"
                    >
                      <FontAwesomeIcon icon={faCopy} />
                    </Button>
                  </div>
                  <small className="text-muted mt-2 d-block">
                    Keep this token secure. It provides API access for your
                    mobile applications.
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
