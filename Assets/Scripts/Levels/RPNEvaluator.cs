using System.Collections.Generic;

public static class RPNEvaluator
{
   public static int Evaluate(string expr, Dictionary<string, int> variables)
   {
      Stack<int> stack = new Stack<int>();
      string[] tokens = expr.Split(' ');

      foreach (var token in tokens)
      {
         if (int.TryParse(token, out int num))
            stack.Push(num);
         else if (variables.ContainsKey(token))
            stack.Push(variables[token]);
         else
         {
            int b = stack.Pop();
            int a = stack.Pop();
            stack.Push(token switch
            {
               "+" => a + b,
               "-" => a - b,
               "*" => a * b,
               "/" => a / b,
               "%" => a % b,
               _ => throw new System.Exception("Unknown operator: " + token)
            });
         }
      }

      return stack.Pop();
   }
}
