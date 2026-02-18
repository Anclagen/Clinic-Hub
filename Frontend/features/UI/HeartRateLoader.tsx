type HeartRateLoaderProps = {
  className?: string;
};

export function HeartRateLoader({ className }: HeartRateLoaderProps) {
  return (
    <div className={["heart-rate-loader", className].filter(Boolean).join(" ")} aria-hidden="true">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 150 73" className="h-full w-full">
        <polyline
          className="heart-rate-loader__path"
          pathLength={100}
          fill="none"
          stroke="currentColor"
          strokeWidth="3"
          strokeMiterlimit="10"
          points="0,45.486 38.514,45.486 44.595,33.324 50.676,45.486 57.771,45.486 62.838,55.622 71.959,9 80.067,63.729 84.122,45.486 97.297,45.486 103.379,40.419 110.473,45.486 150,45.486"
        />
      </svg>
    </div>
  );
}
