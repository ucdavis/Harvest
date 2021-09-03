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

  const duplicateActivity = (activity: Activity) => {
    const newActivity = { ...activity };

    newActivity.id = Math.max(...props.quote.activities.map((a) => a.id)) + 1;
    newActivity.name = `${activity.name} (1)`;
    newActivity.workItems = [...activity.workItems];

    props.updateQuote({
      ...props.quote,
      activities: [...props.quote.activities, newActivity],
    });
  };

  return (
    <div>
      {props.quote.activities.map((activity) => (
        <ActivityForm
          key={`activity-${activity.id}`}
          activity={activity}
          duplicateActivity={(activity: Activity) =>
            duplicateActivity(activity)
          }
          updateActivity={(activity: Activity) => updateActivity(activity)}
          deleteActivity={(activity: Activity) => deleteActivity(activity)}
          rates={props.rates}
          years={props.quote.years}
        />
      ))}
    </div>
  );
};
