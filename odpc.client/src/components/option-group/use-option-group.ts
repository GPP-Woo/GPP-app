import { ref, watchEffect } from "vue";

export const useOptionGroup = () => {
  const groupRef = ref<HTMLElement>();

  const getMessage = (type: string) =>
    type === "radio" ? "Kies één optie." : "Kies minimaal één optie.";

  const isAnyChecked = (options: NodeListOf<HTMLInputElement>) =>
    Array.from(options).some((option) => option.checked);

  const setCustomValidity = () => {
    const required = groupRef.value?.hasAttribute("aria-required");

    const options = (groupRef.value?.querySelectorAll("[type='checkbox'], [type='radio']") ||
      []) as NodeListOf<HTMLInputElement>;

    options.forEach((option) =>
      option.setCustomValidity(required && !isAnyChecked(options) ? getMessage(option.type) : "")
    );
  };

  watchEffect(() => setCustomValidity());

  return {
    groupRef,
    setCustomValidity,
    getMessage
  };
};
