export const isFirstFrame = (time: string): boolean => {
  const parts = time.split(":");
  if (parts.length === 2) {
    return parseInt(parts[0]) === 0 && parseInt(parts[1]) === 0;
  } else if (parts.length === 3) {
    return parseInt(parts[1]) === 0 && parseInt(parts[2]) === 0;
  }
  return false;
};
