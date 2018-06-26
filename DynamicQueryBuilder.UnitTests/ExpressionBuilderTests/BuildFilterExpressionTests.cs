﻿// <copyright file="BuildFilterExpressionTests.cs" company="Oplog">
// Copyright (c) Oplog. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using static DynamicQueryBuilder.DynamicQueryBuilderExceptions;

namespace DynamicQueryBuilder.UnitTests.ExpressionBuilderTests
{
    public class BuildFilterExpressionTests : TestBase
    {
        [Fact]
        public void ShouldThrowExceptionWhenInvalidFilterValueGiven()
        {
            Assert.Throws<DynamicQueryException>(() =>
            {
                ExpressionBuilder.BuildFilterExpression<MemberTestClass>(
                    XParam, new Filter { Value = null, PropertyName = "Name", Operator = FilterOperation.In });
            });
        }

        [Fact]
        public void ShouldConvertNullAsStringToTypeAsString()
        {
            ExpressionBuilder.BuildFilterExpression<MemberTestClass>(
                XParam, new Filter { Value = "null", PropertyName = "Name", Operator = FilterOperation.Equals });
        }

        [Fact]
        public void ShouldConvertInOperationToMultipleEquals()
        {
            const string resultOfQuery = "(((x.Name == \"te\") Or (x.Name == \" test\")) Or (x.Name == \" testx\"))";
            Expression result = ExpressionBuilder.BuildFilterExpression<MemberTestClass>(
                XParam,
                new Filter { Value = "te, test, testx", PropertyName = "Name", Operator = FilterOperation.In });

            Assert.Equal(result.ToString(), resultOfQuery);
        }

        [Fact]
        public void ShouldHandleNullParamterValues()
        {
            const string resultOfQuery = "(x.Name == \"\")";
            Expression result = ExpressionBuilder.BuildFilterExpression<MemberTestClass>(
                XParam,
                new Filter { Value = null, PropertyName = "Name", Operator = FilterOperation.Equals });

            Assert.Equal(result.ToString(), resultOfQuery);
        }

        [Fact]
        public void ShouldHandleEveryFilterOperationSupported()
        {
            const string resultOfEquals = "(x.Name == \"test\")";
            const string resultOfLessThan = "(x.InnerMember.Age < 3)";
            const string resultOfContains = "x.Name.Contains(\"test\")";
            const string resultOfNotEquals = "(x.Name != \"test\")";
            const string resultOfEndsWith = "x.Name.EndsWith(\"test\")";
            const string resultOfEStartsWith = "x.Name.StartsWith(\"test\")";
            const string resultOfGreaterThan = "(x.InnerMember.Age > 3)";
            const string resultOfLessThanOrEquals = "(x.InnerMember.Age <= 3)";
            const string resultOfGreaterThanOrEquals = "(x.InnerMember.Age >= 3)";

            Expression result = Expression.Empty();
            List<string> operations = Enum.GetNames(typeof(FilterOperation)).ToList();
            operations.Remove(nameof(FilterOperation.In)); // We handle this differently

            foreach (string item in operations)
            {
                var innerOperation = Enum.Parse<FilterOperation>(item);
                switch (innerOperation)
                {
                    case FilterOperation.Equals:
                        Assert.Equal(resultOfEquals, BuildQuery(FilterOperation.Equals));
                        break;
                    case FilterOperation.LessThan:
                        Assert.Equal(resultOfLessThan, BuildQuery(FilterOperation.LessThan, "3", "InnerMember.Age"));
                        break;
                    case FilterOperation.Contains:
                        Assert.Equal(resultOfContains, BuildQuery(FilterOperation.Contains));
                        break;
                    case FilterOperation.NotEqual:
                        Assert.Equal(resultOfNotEquals, BuildQuery(FilterOperation.NotEqual));
                        break;
                    case FilterOperation.EndsWith:
                        Assert.Equal(resultOfEndsWith, BuildQuery(FilterOperation.EndsWith));
                        break;
                    case FilterOperation.StartsWith:
                        Assert.Equal(resultOfEStartsWith, BuildQuery(FilterOperation.StartsWith));
                        break;
                    case FilterOperation.GreaterThan:
                        Assert.Equal(resultOfGreaterThan, BuildQuery(FilterOperation.GreaterThan, "3", "InnerMember.Age"));
                        break;
                    case FilterOperation.LessThanOrEqual:
                        Assert.Equal(resultOfLessThanOrEquals, BuildQuery(FilterOperation.LessThanOrEqual, "3", "InnerMember.Age"));
                        break;
                    case FilterOperation.GreaterThanOrEqual:
                        Assert.Equal(resultOfGreaterThanOrEquals, BuildQuery(FilterOperation.GreaterThanOrEqual, "3", "InnerMember.Age"));
                        break;
                    default:
                        Assert.Null(BuildQuery((FilterOperation)999));
                        break;
                }
            }
        }

        [Fact]
        public void ShouldReturnRullWhenNotSupportedOperationPassed() // don't know how this would happen tho.
        {
            Expression result = ExpressionBuilder.BuildFilterExpression<MemberTestClass>(
            XParam, new Filter { Value = "test", PropertyName = "Name", Operator = (FilterOperation)999 });
            Assert.Null(result);
        }

        private string BuildQuery(FilterOperation operation, string value = "test", string propName = "Name")
        {
            return ExpressionBuilder.BuildFilterExpression<MemberTestClass>(
                        XParam,
                        new Filter { Value = value, PropertyName = propName, Operator = operation })?.ToString();
        }
    }
}
