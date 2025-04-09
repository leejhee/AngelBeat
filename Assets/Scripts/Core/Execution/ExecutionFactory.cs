public static class ExecutionFactory
{
    //execution type에 따라 만들어야함. 종류 많을 예정
      
    public static ExecutionBase ExecutionGenerate(ExecutionParameter buffParam)
    {
        switch (buffParam.eExecutionType)
        {
            default:
                return null;
        }

    }

    
}