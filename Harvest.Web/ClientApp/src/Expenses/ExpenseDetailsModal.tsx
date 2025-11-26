import React from "react";
import {
  Button,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Table,
} from "reactstrap";
import { Expense } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  expense: Expense | null;
  isOpen: boolean;
  toggle: () => void;
}

export const ExpenseDetailsModal = ({ expense, isOpen, toggle }: Props) => {
  if (!expense) return null;

  return (
    <Modal isOpen={isOpen} toggle={toggle} size="lg">
      <ModalHeader>Expense Details</ModalHeader>
      <ModalBody>
        <Table bordered>
          <tbody>
            <tr>
              <th>Activity</th>
              <td>{expense.activity}</td>
            </tr>
            <tr>
              <th>Type</th>
              <td>{expense.type}</td>
            </tr>
            <tr>
              <th>Description</th>
              <td>{expense.description}</td>
            </tr>
            <tr>
              <th>Quantity</th>
              <td>{expense.quantity}</td>
            </tr>
            <tr>
              <th>Markup</th>
              <td>{expense.markup ? "Yes" : "No"}</td>
            </tr>
            <tr>
                <th>Passthrough</th>
                <td>{expense.isPassthrough ? "Yes" : "No"}</td>
            </tr>
            <tr>
              <th>Rate</th>
              <td>${formatCurrency(expense.price)}</td>
            </tr>
            <tr>
              <th>Total</th>
              <td>${formatCurrency(expense.total)}</td>
            </tr>
            <tr>
              <th>Created On</th>
              <td>
                {expense.createdOn
                  ? new Date(expense.createdOn).toLocaleDateString()
                  : "N/A"}
              </td>
            </tr>
            <tr>
              <th>Created By</th>
              <td>{expense.createdBy?.name || "N/A"}</td>
            </tr>
            <tr>
              <th>Approved</th>
              <td>{expense.approved ? "Yes" : "No"}</td>
            </tr>
            <tr>
              <th>Approved By</th>
              <td>{expense.approvedBy?.name || "N/A"}</td>
            </tr>
            <tr>
              <th>Approved On</th>
              <td>
                {expense.approvedOn
                  ? new Date(expense.approvedOn).toLocaleDateString()
                  : "N/A"}
              </td>
            </tr>
            {expense.project && (
              <tr>
                <th>Project</th>
                <td>
                  {expense.project.name} ({expense.project.id})
                </td>
              </tr>
            )}
          </tbody>
        </Table>
      </ModalBody>
      <ModalFooter>
        <Button color="secondary" onClick={toggle}>
          Close
        </Button>
      </ModalFooter>
    </Modal>
  );
};
