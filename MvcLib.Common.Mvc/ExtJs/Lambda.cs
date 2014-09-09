using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MvcLib.Common.Mvc.ExtJs
{
    public static class Lambda
    {
        public static string ExtractMemberName(LambdaExpression lambda)
        {
            var key = string.Empty;

            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Equal:
                    {
                        var binary = (BinaryExpression)lambda.Body;
                        var left = binary.Left;
                        var member = (MemberExpression)left;
                        key = member.Member.Name;
                    }
                    break;
                case ExpressionType.Call:
                    {
                        var call = (MethodCallExpression)lambda.Body;
                        var memberExpression = call.Object as MemberExpression;
                        if (memberExpression != null)
                        {
                            key = memberExpression.Member.Name;
                        }
                    }
                    break;
                case ExpressionType.MemberAccess:
                    {
                        var expr = (MemberExpression)lambda.Body;
                        key = expr.Member.Name;
                    }
                    break;

                case ExpressionType.Convert:
                    {
                        var expr = (UnaryExpression)lambda.Body;

                        MemberExpression memberExpression;

                        var methodCallExpression = expr.Operand as MethodCallExpression;
                        if (methodCallExpression != null)
                        {
                            memberExpression = methodCallExpression.Object as MemberExpression;
                        }
                        else
                        {
                            memberExpression = (MemberExpression)expr.Operand;
                        }
                        if (memberExpression != null) key = memberExpression.Member.Name;
                    }
                    break;
            }

            return key;
        }

        /// <summary>
        /// Cria uma nova instância de expressão lambda, substituindo o valor "template" com um valor atualizado
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression<Func<TEntity, bool>> ModifyExpression<TEntity>(Expression<Func<TEntity, object>> expression, object value)
        {
            var lambda = (LambdaExpression)expression;
            var parameterExpr = lambda.Parameters[0];

            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Equal:// x => x.Id == 1
                    {
                        var binary = (BinaryExpression)lambda.Body;
                        var left = binary.Left;
                        var member = (MemberExpression)left;
                        var type = member.Type;
                        var right = Expression.Constant(Convert.ChangeType(value, type));
                        var newBinary = Expression.MakeBinary(ExpressionType.Equal, left, right);

                        return Expression.Lambda<Func<TEntity, bool>>(newBinary, parameterExpr);
                    }
                case ExpressionType.Call: // x => x.Nome.Contains("value")
                    {
                        var call = (MethodCallExpression)lambda.Body;
                        var type = call.Arguments[0].Type;
                        var arg = Expression.Constant(Convert.ChangeType(value, type));

                        var newCall = Expression.Call(call.Object, call.Method, new Expression[] { arg });
                        return Expression.Lambda<Func<TEntity, bool>>(newCall, parameterExpr);
                    }
                case ExpressionType.Convert: //x => x.Id
                    {
                        var unary = (UnaryExpression)lambda.Body;
                        var operand = unary.Operand;

                        if (operand is MethodCallExpression)
                        {
                            var call = (MethodCallExpression)operand;
                            var type = call.Arguments[0].Type;
                            var arg = Expression.Constant(Convert.ChangeType(value, type));

                            var newCall = Expression.Call(call.Object, call.Method, new Expression[] { arg });
                            return Expression.Lambda<Func<TEntity, bool>>(newCall, parameterExpr);
                        }
                        else
                        {
                            var member = (MemberExpression)operand;

                            var nonNullableType = Nullable.GetUnderlyingType(member.Type);

                            object convertedValue;

                            if (nonNullableType != null)
                            {
                                var converter = new NullableConverter(member.Type);
                                convertedValue = Convert.ChangeType(value, converter.UnderlyingType);
                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(value, member.Type);
                            }

                            var right = Expression.Constant(convertedValue, member.Type);
                            var newBinary = Expression.MakeBinary(ExpressionType.Equal, operand, right);

                            return Expression.Lambda<Func<TEntity, bool>>(newBinary, parameterExpr);
                        }
                    }
                case ExpressionType.MemberAccess: // x => x.Status
                    {
                        var unary = (UnaryExpression)lambda.Body;
                        var operand = unary.Operand;
                        var member = (MemberExpression)operand;
                        var type = member.Type;
                        var right = Expression.Constant(Convert.ChangeType(value, type));
                        var newBinary = Expression.MakeBinary(ExpressionType.Equal, operand, right);

                        return Expression.Lambda<Func<TEntity, bool>>(newBinary, parameterExpr);

                    }
            }

            return null;
        }

        //todo: Este método deve tratar a expressao order by corretamente.
        /*
         * atualmente o que ocorre:
         * ao gravar a expressão no dicionário, a lambda está sendo passada como Func<TEntity, object>.
         * Deve-se criar uma expressão lambda usando MakeGenericType para que retorne Func<TEntity, TKey>
         * Caso contrário na hora da execução, o LinQ irá lançar uma exception de conversão (object)TKey
        */
        public static Expression<Func<TEntity, TArg>> CheckExpression<TEntity, TArg>(Expression<Func<TEntity, TArg>> expression)
        {
            //var lambda = (LambdaExpression)expression;
            //var parameterExpr = lambda.Parameters[0];

            //switch (lambda.Body.NodeType)
            //{
            //    case ExpressionType.Convert: //x => x.Id
            //        {
            //            var unary = (UnaryExpression)lambda.Body;
            //            var operand = unary.Operand;

            //            if (operand is MethodCallExpression)
            //            {
            //                var call = (MethodCallExpression)operand;

            //                return Expression.Lambda<Func<TEntity, TArg>>(call, parameterExpr);
            //            }

            //            if (operand is MemberExpression)
            //            {
            //                var ex = Expression.Lambda<Func<TEntity, TArg>>(operand, parameterExpr);
            //                return ex;
            //            }

            //            goto default;
            //        }

            //    default:
            //        {
            //            return expression;
            //        }
            //}

            return expression;
        }
    }
}