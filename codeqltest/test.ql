import csharp

// find all templates
from Method mapRoute, Call call, Expr conventionalRouteTemplates
where
  mapRoute.hasName("MapControllerRoute") and
  call.getTarget() = mapRoute and
  DataFlow::localFlow(DataFlow::exprNode(conventionalRouteTemplates),
    DataFlow::exprNode(call.getArgumentForName("pattern"))) and
  conventionalRouteTemplates.getType() instanceof StringType and
  conventionalRouteTemplates.hasValue()
select mapRoute, call, conventionalRouteTemplates, conventionalRouteTemplates.getType()
