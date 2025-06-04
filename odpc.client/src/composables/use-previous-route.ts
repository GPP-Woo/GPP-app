import { ref } from "vue";
import { type RouteLocationNormalizedLoadedGeneric } from "vue-router";

const previousRoute = ref<RouteLocationNormalizedLoadedGeneric>();

export const usePreviousRoute = () => ({ previousRoute });

export const setPreviousRoute = (route: RouteLocationNormalizedLoadedGeneric) =>
  (previousRoute.value = route);
