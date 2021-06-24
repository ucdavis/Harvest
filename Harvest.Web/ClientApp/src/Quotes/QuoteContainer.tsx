import React, { useEffect, useMemo, useState, useRef } from "react";
import { useHistory, useParams } from "react-router-dom";
import { useFormik, FormikConfig, FormikHelpers, FieldArray, FieldArrayRenderProps, FormikProvider } from "formik";
import { getInputValidityStyle, ValidationErrorMessage, FormikState } from "../Validation";
import { quoteContentSchema } from "../schemas";

import {
  Project,
  ProjectWithQuote,
  QuoteContent,
  QuoteContentImpl,
  Rate,
  WorkItemImpl
} from "../types";

import { FieldContainer } from "../Fields/FieldContainer";
import { ProjectDetail } from "./ProjectDetail";
import { ProjectHeader } from "../Requests/ProjectHeader";
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

  const activitiesRef = useRef<FieldArrayRenderProps>(null);


  const handleSubmit = async (quote: QuoteContent, actions: FormikHelpers<QuoteContent>) => {
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

    actions.setSubmitting(false);

    if (saveResponse.ok) {
      history.push(`/Project/Details/${projectId}`);
    } else {
      alert("Something went wrong, please try again");
    }
  }

  const formik = useFormik<QuoteContent>({
    initialValues: new QuoteContentImpl(),
    validationSchema: quoteContentSchema,
    onSubmit: handleSubmit
  } as FormikConfig<QuoteContent>);

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
          formik.setValues({
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

          formik.setValues(quoteToUse);
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
    const newActivityId = Math.max(...formik.values.activities.map((a) => a.id), 0) + 1;
    activitiesRef.current?.push({
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
      <FormikProvider value={formik}>
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
            <FieldArray name="fields">
              {(arrayHelpers) => (
                <FieldContainer
                  crops={cropArray}
                  fields={formik.values.fields}
                  updateField={(field) => arrayHelpers.replace(formik.values.fields.findIndex(f => f.id === field.id), { ...field })}
                  addField={(field) => arrayHelpers.push({ ...field })}
                  removeField={(field) => arrayHelpers.remove(formik.values.fields.findIndex(f => f.id === field.id))}
                ></FieldContainer>
              )}
            </FieldArray>
          </div>
          <div>Debug: {JSON.stringify(formik.values)}</div>
        </div>
      </FormikProvider>
    );
  }

  return (
    <FormikProvider value={formik}>
      <div className="card-wrapper">
        <ProjectHeader project={project} title={"Field Request #" + (project?.id || "")}/>
        <div className="card-green-bg">
          <div className="card-content">
            <div className="quote-details">
              <h2>Quote Details</h2>
              <hr />
              <ProjectDetail
                rates={rates}
                formik={formik}
                setEditFields={setEditFields}
                addActivity={addActivity}
              />
              <ActivitiesContainer
                ref={activitiesRef}
                formik={formik}
                rates={rates}
              />
            </div>
            <QuoteTotals quote={formik.values}></QuoteTotals>

            <button className="btn btn-primary mt-4" onClick={() => {
              if (Object.keys(formik.errors).length > 0) {
                alert(JSON.stringify(formik.errors, null, "  "));
              } else {
                formik.submitForm();
              }
            }}>
              Save Quote
          </button>
          </div>
        </div>

        <div>Debug: {JSON.stringify(formik.values)}</div>
        <div>Debug Rates: {JSON.stringify(rates)}</div>
      </div>
    </FormikProvider>
  );
};
