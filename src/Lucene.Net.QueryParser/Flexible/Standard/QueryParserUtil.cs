﻿using Lucene.Net.Analysis;
using Lucene.Net.Search;
using System;
using System.Text;

namespace Lucene.Net.QueryParsers.Flexible.Standard
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// This class defines utility methods to (help) parse query strings into
    /// <see cref="Query"/> objects.
    /// </summary>
    public sealed class QueryParserUtil
    {
        /// <summary>
        /// Parses a query which searches on the fields specified.
        /// <para/>
        /// If x fields are specified, this effectively constructs:
        /// <code>
        /// (field1:query1) (field2:query2) (field3:query3)...(fieldx:queryx)
        /// </code>
        /// </summary>
        /// <param name="queries">Queries strings to parse</param>
        /// <param name="fields">Fields to search on</param>
        /// <param name="analyzer">Analyzer to use</param>
        /// <exception cref="ArgumentException">
        /// if the length of the queries array differs from the length of the
        /// fields array
        /// </exception>
        public static Query Parse(string[] queries, string[] fields, Analyzer analyzer)
        {
            if (queries.Length != fields.Length)
                throw new ArgumentException("queries.length != fields.length");
            BooleanQuery bQuery = new BooleanQuery();

            StandardQueryParser qp = new StandardQueryParser();
            qp.Analyzer = analyzer;

            for (int i = 0; i < fields.Length; i++)
            {
                Query q = qp.Parse(queries[i], fields[i]);

                if (q != null && // q never null, just being defensive
                    (!(q is BooleanQuery) || ((BooleanQuery)q).GetClauses().Count > 0))
                {
                    bQuery.Add(q, BooleanClause.Occur.SHOULD);
                }
            }
            return bQuery;
        }

        /// <summary>
        /// Parses a query, searching on the fields specified. Use this if you need to
        /// specify certain fields as required, and others as prohibited.
        /// <para/>
        /// Usage:
        /// <code>
        /// string[] fields = {&quot;filename&quot;, &quot;contents&quot;, &quot;description&quot;};
        /// BooleanClause.Occur[] flags = {BooleanClause.Occur.SHOULD,
        ///     BooleanClause.Occur.MUST,
        ///     BooleanClause.Occur.MUST_NOT};
        /// MultiFieldQueryParser.Parse(&quot;query&quot;, fields, flags, analyzer);
        /// </code>
        /// <para/>
        /// The code above would construct a query:
        /// <code>
        /// (filename:query) +(contents:query) -(description:query)
        /// </code>
        /// </summary>
        /// <param name="query">Query string to parse</param>
        /// <param name="fields">Fields to search on</param>
        /// <param name="flags">Flags describing the fields</param>
        /// <param name="analyzer">Analyzer to use</param>
        /// <exception cref="ArgumentException">
        /// if the length of the fields array differs from the length of the
        /// flags array
        /// </exception>
        public static Query Parse(string query, string[] fields,
            BooleanClause.Occur[] flags, Analyzer analyzer)
        {
            if (fields.Length != flags.Length)
                throw new ArgumentException("fields.length != flags.length");
            BooleanQuery bQuery = new BooleanQuery();

            StandardQueryParser qp = new StandardQueryParser();
            qp.Analyzer = analyzer;

            for (int i = 0; i < fields.Length; i++)
            {
                Query q = qp.Parse(query, fields[i]);

                if (q != null && // q never null, just being defensive
                    (!(q is BooleanQuery) || ((BooleanQuery)q).GetClauses().Count > 0))
                {
                    bQuery.Add(q, flags[i]);
                }
            }
            return bQuery;
        }

        /// <summary>
        /// Parses a query, searching on the fields specified. Use this if you need to
        /// specify certain fields as required, and others as prohibited.
        /// <para/>
        /// Usage:
        /// <code>
        /// string[] query = {&quot;query1&quot;, &quot;query2&quot;, &quot;query3&quot;};
        /// string[] fields = {&quot;filename&quot;, &quot;contents&quot;, &quot;description&quot;};
        /// BooleanClause.Occur[] flags = {BooleanClause.Occur.SHOULD,
        ///     BooleanClause.Occur.MUST,
        ///     BooleanClause.Occur.MUST_NOT};
        /// MultiFieldQueryParser.Parse(query, fields, flags, analyzer);
        /// </code>
        /// <para/>
        /// The code above would construct a query:
        /// <code>
        /// (filename:query1) +(contents:query2) -(description:query3)
        /// </code>
        /// </summary>
        /// <param name="queries">Queries string to parse</param>
        /// <param name="fields">Fields to search on</param>
        /// <param name="flags">Flags describing the fields</param>
        /// <param name="analyzer">Analyzer to use</param>
        /// <exception cref="ArgumentException">
        /// if the length of the queries, fields, and flags array differ
        /// </exception>
        public static Query Parse(string[] queries, string[] fields,
            BooleanClause.Occur[] flags, Analyzer analyzer)
        {
            if (!(queries.Length == fields.Length && queries.Length == flags.Length))
                throw new ArgumentException(
                    "queries, fields, and flags array have have different length");
            BooleanQuery bQuery = new BooleanQuery();

            StandardQueryParser qp = new StandardQueryParser();
            qp.Analyzer = analyzer;

            for (int i = 0; i < fields.Length; i++)
            {
                Query q = qp.Parse(queries[i], fields[i]);

                if (q != null && // q never null, just being defensive
                    (!(q is BooleanQuery) || ((BooleanQuery)q).GetClauses().Count > 0))
                {
                    bQuery.Add(q, flags[i]);
                }
            }
            return bQuery;
        }

        /// <summary>
        /// Returns a string where those characters that TextParser expects to be
        /// escaped are escaped by a preceding <c>\</c>.
        /// </summary>
        public static string Escape(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                // These characters are part of the query syntax and must be escaped
                if (c == '\\' || c == '+' || c == '-' || c == '!' || c == '(' || c == ')'
                    || c == ':' || c == '^' || c == '[' || c == ']' || c == '\"'
                    || c == '{' || c == '}' || c == '~' || c == '*' || c == '?'
                    || c == '|' || c == '&' || c == '/')
                {
                    sb.Append('\\');
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}