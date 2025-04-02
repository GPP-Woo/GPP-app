export default {
  mounted(el: HTMLFormElement) {
    const onInvalid = () => {
      const invalidInputs = (el.querySelectorAll(":user-invalid") ||
        []) as NodeListOf<HTMLInputElement>;

      invalidInputs.forEach((input) => {
        const details = input.closest("details");
        if (details) details.open = true;
      });
    };

    el.addEventListener("invalid", onInvalid, true);
    el.onInvalidHandler = onInvalid;
  },
  unmounted(el: HTMLFormElement) {
    if (el.onInvalidHandler) {
      el.removeEventListener("invalid", el.onInvalidHandler, true);
      delete el.onInvalidHandler;
    }
  }
};
