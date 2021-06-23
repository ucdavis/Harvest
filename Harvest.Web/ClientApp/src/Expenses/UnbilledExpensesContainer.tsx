import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Activity, Expense, Rate, WorkItem } from "../types";

interface RouteParams {
  projectId?: string;
}

export const UnbilledExpensesContainer = () => {
  const { projectId } = useParams<RouteParams>();

  const [rates, setRates] = useState<Rate[]>([]);
  const [activities, setActivities] = useState<Activity[]>([]);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch("/Rate/Active");

      if (response.ok) {
        const rates: Rate[] = await response.json();

        setRates(rates);
      }
    };

    cb();
  }, []);

  // Get all unbilled expenses for the given project and put them into activities
  useEffect(() => {
    if (projectId === undefined) return;

    const cb = async () => {
      const response = await fetch(`/Expense/GetUnbilled/${projectId}`);

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        if (expenses.length > 0) {
          const uniqueActivities = Array.from(
            new Set(expenses.map((e) => e.activity))
          );

          const activitiesWithWorkItems = uniqueActivities.map((val, idx) => {
            const activityItems = expenses
              .filter((e) => e.activity === val)
              .map((e) => {
                const itemRate = rates.find((r) => r.id === e.rateId);

                const item = {
                  activityId: idx,
                  description: e.description,
                  quantity: e.quantity,
                  total: e.total,
                  rate: e.price,
                  rateId: e.rateId,
                  unit: itemRate?.unit, // TODO: we should probably just store unit in the db
                  type: e.type,
                } as WorkItem;

                return item;
              });
            const total = activityItems.reduce(
              (prev, curr) => prev + curr.total,
              0
            );

            const activity: Activity = {
              id: idx,
              name: val,
              total,
              workItems: activityItems,
            };

            return activity;
          });

          setActivities(activitiesWithWorkItems);
        } else {
          setActivities([]);
        }
      }
    };

    cb();
  }, [projectId, rates]);

  return (
    <div>
      <h3>Unbilled!</h3>
      {projectId}
      {JSON.stringify(activities)}
    </div>
  );
};
