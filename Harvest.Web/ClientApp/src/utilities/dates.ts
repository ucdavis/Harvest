export const parseIsoDate = (s?: string) => {
  if (!s)
    return new Date();
  const b = s.split(/\D+/);
  console.log(JSON.stringify(b));
  return new Date(Date.UTC(parseInt(b[0]), parseInt(b[1]) - 1, parseInt(b[2]), parseInt(b[3]), parseInt(b[4]), parseInt(b[5]), parseInt(b[6])));
}
