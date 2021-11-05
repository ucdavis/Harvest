export const ensureUTC = (date: Date) => {
  if (date.toString().includes("Z")) {
    return new Date(date);
  }

  return new Date(date.toString() + "Z");
};
