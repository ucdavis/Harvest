export const EnsureUTC = (date: Date) => {
  if (date.toString().includes("Z")) {
    return new Date(date);
  }

  return new Date(date + "Z");
};
