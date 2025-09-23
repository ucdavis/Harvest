import React from "react";
import ReactDOM from "react-dom";
import { RateQRCodeGenerator } from "../QR/RateQRCodeGenerator";
import { Rate } from "../../types";

interface MvcRateData {
  id: number;
  description: string;
  type: string;
  price: number;
  unit: string;
  isPassthrough: boolean;
}

interface RateQRMountOptions {
  rateData: MvcRateData;
  teamSlug: string;
  teamName: string;
  containerId: string;
}

/**
 * Mounts a Rate QR Code generator component in a MVC view
 * This function is called from MVC Razor views to embed React QR functionality
 */
export const mountRateQRCode = (options: RateQRMountOptions): void => {
  const { rateData, teamSlug, teamName, containerId } = options;
  const container = document.getElementById(containerId);

  if (!container) {
    console.error(`Container with id '${containerId}' not found`);
    return;
  }

  // Convert MVC data format to React Rate interface
  const rate: Rate = {
    id: rateData.id,
    description: rateData.description,
    type: rateData.type as any, // Type assertion since MVC sends string
    price: rateData.price,
    unit: rateData.unit,
    isPassthrough: rateData.isPassthrough,
  };

  const handleClose = () => {
    ReactDOM.unmountComponentAtNode(container);
    // Hide the container after unmounting
    container.style.display = "none";
  };

  // Show the container and mount the component
  container.style.display = "block";

  ReactDOM.render(
    <RateQRCodeGenerator
      rate={rate}
      teamInfo={{ name: teamName, slug: teamSlug }}
      onClose={handleClose}
    />,
    container
  );
};

/**
 * Unmounts any React component from the specified container
 */
export const unmountRateQRCode = (containerId: string): void => {
  const container = document.getElementById(containerId);
  if (container) {
    ReactDOM.unmountComponentAtNode(container);
    container.style.display = "none";
  }
};

// Make functions available globally for MVC views
declare global {
  interface Window {
    HarvestMvcHelpers: {
      mountRateQRCode: typeof mountRateQRCode;
      unmountRateQRCode: typeof unmountRateQRCode;
    };
  }
}

// Initialize global helpers
if (typeof window !== "undefined") {
  window.HarvestMvcHelpers = window.HarvestMvcHelpers || {};
  window.HarvestMvcHelpers.mountRateQRCode = mountRateQRCode;
  window.HarvestMvcHelpers.unmountRateQRCode = unmountRateQRCode;
}
