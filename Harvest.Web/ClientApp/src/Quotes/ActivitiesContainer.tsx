import React from "react";

import { Activity, QuoteContent, Rate } from "../types";

import { ActivityForm } from "./ActivityForm";

interface Props {
  rates: Rate[];
  quote: QuoteContent;
  updateQuote: React.Dispatch<React.SetStateAction<QuoteContent>>;
}

export const ActivitiesContainer = (props: Props) => {
  const updateActivity = (activity: Activity) => {
    // TODO: can we get away without needing to spread copy?  do we need to totally splice/replace?
    const allActivities = props.quote.activities;
    const activityIndex = allActivities.findIndex((a) => a.id === activity.id);
    allActivities[activityIndex] = {
      ...activity,
      total: activity.workItems.reduce(
        (prev, curr) => prev + curr.total || 0,
        0
      ),
    };

    props.updateQuote({ ...props.quote, activities: [...allActivities] });
  };
  const deleteActivity = (activity: Activity) => {
    const allActivities = props.quote.activities.filter(
      (a) => a.id !== activity.id
    );
    props.updateQuote({ ...props.quote, activities: [...allActivities] });
  };

  return (
    <div>
      {props.quote.activities.map((activity) => (
        <ActivityForm
          key={`activity-${activity.id}`}
          activity={activity}
          updateActivity={(activity: Activity) => updateActivity(activity)}
          deleteActivity={(activity: Activity) => deleteActivity(activity)}
          rates={props.rates}
          years={props.quote.years}
        />
      ))}
    </div>
  );
};
