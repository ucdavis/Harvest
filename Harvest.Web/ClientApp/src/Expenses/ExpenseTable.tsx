import React, { useMemo, useState, useCallback } from "react";
import { Cell, Column, TableState } from "react-table";
import { Button, UncontrolledTooltip } from "reactstrap";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { CommonRouteParams, Expense } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { ShowFor } from "../Shared/ShowFor";
import { ExpenseDetailsModal } from "./ExpenseDetailsModal";
import { useParams, useHistory } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck, faTrash, faEdit } from "@fortawesome/free-solid-svg-icons";

interface Props {
  expenses: Expense[];
  deleteExpense: (expense: Expense) => void;
  showActions: boolean;
  showProject: boolean;
  showApprove: boolean;
  approveExpense?: (expense: Expense) => void;
  showExport?: boolean;
  showAll?: boolean; // indicates if we're showing all expenses or just user's workers
}

export const ExpenseTable = (props: Props) => {
  const expenseData = useMemo(() => props.expenses, [props.expenses]);
  const [selectedExpense, setSelectedExpense] = useState<Expense | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [showProject] = useState(props.showProject);
  const { team, projectId } = useParams<CommonRouteParams>();
  const history = useHistory();

  const { deleteExpense, showActions, approveExpense, showApprove } = props;

  const handleRowClick = (expense: Expense) => {
    setSelectedExpense(expense);
    setIsModalOpen(true);
  };

  const toggleModal = () => {
    setIsModalOpen(!isModalOpen);
    if (isModalOpen) {
      setSelectedExpense(null);
    }
  };

  const handleEditExpense = useCallback(
    (expense: Expense) => {
      // Try to get project ID from the current route params first, then from expense
      const targetProjectId = projectId || expense.project?.id;
      if (targetProjectId) {
        // Add query parameters to indicate return path
        const searchParams = new URLSearchParams();
        searchParams.set("returnOnSubmit", "true");
        if (props.showAll !== undefined) {
          searchParams.set("returnToShowAll", props.showAll.toString());
        }

        history.push(
          `/${team}/expense/edit/${targetProjectId}/${
            expense.id
          }?${searchParams.toString()}`
        );
      }
    },
    [history, team, projectId, props.showAll]
  );

  const columns: Column<Expense>[] = useMemo(
    () => [
      ...(showProject
        ? [
            {
              Header: "Project",
              accessor: (row: Expense) => row.project?.name,
              Cell: (data: Cell<Expense>) =>
                data.row.original.project ? (
                  <a
                    href={`/${team}/Project/Details/${data.row.original.project.id}`}
                  >
                    {data.row.original.project.name}
                  </a>
                ) : (
                  "N/A"
                ),
            },
          ]
        : []),
      {
        Header: "Activity",
        accessor: (row) => row.activity || "N/A",
      },
      {
        Header: "Type",
        accessor: (row) => row.type,
      },
      {
        Header: "Description",
        accessor: (row) => row.description,
      },
      {
        Header: "Quantity",
        accessor: (row) => row.quantity,
      },
      {
        Header: "Rate",
        accessor: (row) => "$" + formatCurrency(row.price),
      },
      {
        Header: "Total",
        accessor: (row) => "$" + formatCurrency(row.total),
      },
      {
        Header: "Entered by",
        accessor: (row) => row.createdBy?.name,
      },
      {
        Header: "Entered on",
        accessor: (row) =>
          `${row.createdOn} ${
            row.createdOn ? new Date(row.createdOn).toLocaleDateString() : ""
          }`, //This can't be null in the db
        Cell: (data: Cell<Expense>) =>
          data.row.original.createdOn
            ? new Date(data.row.original.createdOn).toLocaleDateString()
            : "N/A",
      },
      {
        Header: "Approved",
        accessor: (row) => (row.approved ? "Yes" : "No"),
      },
      {
        Header: "Approved by",
        accessor: (row) => row.approvedBy?.name,
      },
      {
        Header: "Approved on",
        accessor: (row) =>
          row.approvedOn
            ? new Date(row.approvedOn).toLocaleDateString()
            : "N/A",
      },

      ...(showActions
        ? [
            {
              Header: "Actions",
              Cell: (data: any) => (
                <ShowFor roles={["FieldManager", "Supervisor"]}>
                  <div
                    style={{
                      display: "flex",
                      alignItems: "center",
                    }}
                  >
                    <Button
                      color="link"
                      id={`deleteExpense-${data.row.original.id}`}
                      onClick={() => deleteExpense(data.row.original)}
                      style={{ padding: "0.25rem 0.1rem", margin: "0" }}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                      <UncontrolledTooltip
                        placement="top"
                        target={`deleteExpense-${data.row.original.id}`}
                      >
                        Delete Expense
                      </UncontrolledTooltip>
                    </Button>
                    {showApprove &&
                      !data.row.original.approved &&
                      approveExpense && (
                        <>
                          <Button
                            color="link"
                            id={`approveExpense-${data.row.original.id}`}
                            onClick={() => approveExpense(data.row.original)}
                            style={{
                              padding: "0.25rem 0.1rem",
                              margin: "0",
                              marginLeft: "0.25rem",
                            }}
                          >
                            <FontAwesomeIcon icon={faCheck} />
                            <UncontrolledTooltip
                              placement="top"
                              target={`approveExpense-${data.row.original.id}`}
                            >
                              Approve Expense
                            </UncontrolledTooltip>
                          </Button>
                          <Button
                            color="link"
                            id={`editExpense-${data.row.original.id}`}
                            onClick={() => handleEditExpense(data.row.original)}
                            style={{
                              padding: "0.25rem 0.1rem",
                              margin: "0",
                              marginLeft: "0.25rem",
                            }}
                          >
                            <FontAwesomeIcon icon={faEdit} />
                            <UncontrolledTooltip
                              placement="top"
                              target={`editExpense-${data.row.original.id}`}
                            >
                              Edit Expense
                            </UncontrolledTooltip>
                          </Button>
                        </>
                      )}
                  </div>
                </ShowFor>
              ),
            },
          ]
        : []),
    ],
    [
      deleteExpense,
      showActions,
      approveExpense,
      showApprove,
      showProject,
      team,
      handleEditExpense,
    ]
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "Activity" }, { id: "Type" }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <>
      <ReactTable
        columns={columns}
        data={expenseData}
        initialState={initialState}
        onRowClick={handleRowClick}
        enableExport={props.showExport ?? false}
      />
      <ExpenseDetailsModal
        expense={selectedExpense}
        isOpen={isModalOpen}
        toggle={toggleModal}
      />
    </>
  );
};
