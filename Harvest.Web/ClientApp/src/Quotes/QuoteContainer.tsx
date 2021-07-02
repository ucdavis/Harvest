import React, { useEffect, useMemo, useState, useRef } from "react";
import { useHistory, useParams } from "react-router-dom";
import { useForm, FormProvider, useWatch, useFieldArray, UseFieldArrayReturn } from "react-hook-form";
import { yupResolver } from '@hookform/resolvers/yup';
import { useFormHelpers } from "../Validation";
import { quoteContentSchema } from "../schemas";

import { Project, ProjectWithQuote, QuoteContent, QuoteContentImpl, Rate, WorkItemImpl, Activity, Field } from "../types";

import { FieldContainer } from "../Fields/FieldContainer";
import { ProjectDetail } from "./ProjectDetail";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { ActivitiesContainer } from "./ActivitiesContainer";
import { QuoteTotals } from "./QuoteTotals";

interface RouteParams {
  projectId?: string;
}

export const QuoteContainer = () => {
  const history = useHistory();
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();

  // TODO: set with in-progress quote details if they exist
  // For now, we just always initialize an empty quote
  const [rates, setRates] = useState<Rate[]>([]);

  const [editFields, setEditFields] = useState(false);

  const formMethods = useForm<QuoteContent>({
    defaultValues: new QuoteContentImpl(),
    resolver: yupResolver(quoteContentSchema),
    mode: "onBlur"
  });

  const { control, handleSubmit: submit, formState: { errors }, getValues, reset, setValue } = formMethods;

  const activitiessHelper = useFieldArray({ control, name: "activities", keyName: "fieldId" });

  const [fields, acreageTotal, laborTotal, equipmentTotal, otherTotal, grandTotal] = useWatch<QuoteContent>({
    control: control,
    name: ["fields", "acreageTotal", "laborTotal", "equipmentTotal", "otherTotal", "grandTotal"]
  }) as [Field[], number, number, number, number, number];

  const fieldsHelper = useFieldArray<QuoteContent>({ control: control, name: "fields" });

  const handleSubmit = async (quote: QuoteContent) => {
    // remove unused workitems and empty activities and apply to state only after successfully saving
    quote.activities.forEach((a) => (a.workItems = a.workItems.filter((w) => w.total !== 0)));
    quote.activities = quote.activities.filter((a) => a.total !== 0);

    // TODO: add progress and hide info while saving
    const saveResponse = await fetch(`/Quote/Save/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(quote),
    });

    if (saveResponse.ok) {
      history.push(`/Project/Details/${projectId}`);
    } else {
      alert("Something went wrong, please try again");
    }
  }

  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await fetch(`/Quote/Get/${projectId}`);
      const pricingResponse = await fetch("/Rate/Active");

      if (quoteResponse.ok && pricingResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();
        const rateJson: Rate[] = await pricingResponse.json();
        setProject(projectWithQuote.project);
        setRates(rateJson);

        if (projectWithQuote.quote) {
          // TODO: remove once we standardize on new quote format
          reset({
            ...projectWithQuote.quote,
            fields: projectWithQuote.quote.fields || [],
          });

          if (
            !projectWithQuote.quote.fields ||
            projectWithQuote.quote.fields.length === 0
          ) {
            setEditFields(true);
          }
        } else {
          // TODO: how do we handle if different fields have different rates?
          const quoteToUse = new QuoteContentImpl();
          quoteToUse.acreageRate =
            rateJson.find((r) => r.type === "Acreage")?.price || 120;

          reset({
            ...quoteToUse,
            fields: quoteToUse.fields || [],
          });
          setEditFields(true); // we have no existing quote, start with editing fields
        }
      } else {
        !quoteResponse.ok && console.error(quoteResponse);
        !pricingResponse.ok && console.error(pricingResponse);
      }
    };

    cb();
  }, [projectId]);

  const cropArray = useMemo(() => (project ? project.crop.split(",") : []), [
    project,
  ]);


  const addActivity = () => {
    const activities = getValues("activities");
    const newActivityId = Math.max(...activities.map((a) => a.id), 0) + 1;
    activitiessHelper.append({
      id: newActivityId,
      name: "Activity",
      total: 0,
      workItems: [
        new WorkItemImpl(newActivityId, 1, "Labor"),
        new WorkItemImpl(newActivityId, 2, "Equipment"),
        new WorkItemImpl(newActivityId, 3, "Other"),
      ],
    });
  }

  if (!project) {
    return <div>Loading</div>;
  }

  // TODO: we might want to move this all into a separate component
  if (editFields) {
    return (
      <FormProvider {...formMethods}>
        <div>
          <div className="card-wrapper">
            <ProjectHeader project={project} title={"Field Request #" + (project?.id || "")} />

            <div className="card-green-bg">
              <div className="card-content">
                <div className="row">
                  <div className="col-md-6">
                    <h3>Choose a location</h3>
                  Instructions:
                  <ol>
                      <li>
                        Draw your field boundaries using the rectangle or polygon
                        tool in the upper-right
                    </li>
                      <li>Fill in the field details and click "Confirm"</li>
                      <li>
                        You can add in as many fields as you like, or click an
                        existing field for more info and actions
                    </li>
                      <li>
                        To edit a field, click on it in the map and choose either edit or remove.
                    </li>
                      <li>When you are finished, click confirm below</li>
                    </ol>
                    <button
                      className="btn btn-primary"
                      onClick={(_) => setEditFields(false)}>
                      Confirm Field Locations
                  </button>
                  </div>
                </div>
              </div>
            </div>
            <FieldContainer
              crops={cropArray}
              fields={fields}
              updateField={(field) => setValue(`fields.${fields.findIndex(f => f.id === field.id)}` as "fields.0", { ...field })}
              addField={(field) => fieldsHelper.append({ ...field })}
              removeField={(field) => fieldsHelper.remove(fields.findIndex(f => f.id === field.id))}
            ></FieldContainer>
          </div>
          <div>Debug: {JSON.stringify(getValues())}</div>
        </div>
      </FormProvider>
    );
  }

  return (
    <FormProvider {...formMethods}>
      <div className="card-wrapper">
        <ProjectHeader project={project} title={"Field Request #" + (project?.id || "")} />
        <div className="card-green-bg">
          <div className="card-content">
            <div className="quote-details">
              <h2>Quote Details</h2>
              <hr />
              <ProjectDetail
                rates={rates}
                setEditFields={setEditFields}
                addActivity={addActivity}
              />
              <ActivitiesContainer
                rates={rates}
                activitiesHelper={activitiessHelper}
              />
            </div>
            <QuoteTotals {...{ acreageTotal, laborTotal, equipmentTotal, otherTotal, grandTotal }}></QuoteTotals>

            <button className="btn btn-primary mt-4" onClick={() => {
              if (Object.keys(errors).length > 0) {
                alert(JSON.stringify(errors, null, "  "));
              } else {
                submit(handleSubmit)();
              }
            }}>
              Save Quote
          </button>
          </div>
        </div>

        <div>Debug: {JSON.stringify(getValues())}</div>
        <div>Debug Rates: {JSON.stringify(rates)}</div>
      </div>
    </FormProvider>
  );
};
