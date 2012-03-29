﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration.Operations;
using System.CodeDom.Compiler;
using System.Data;
using hxy.Common.Data;

namespace DbMigration.SqlServer
{
    /// <summary>
    /// SqlServer 的执行项生成器
    /// </summary>
    public class SqlServerRunGenerator : TSqlGenerator
    {
        protected override string ConvertToTypeString(DbType dataType)
        {
            return DbTypeHelper.ConvertToSQLTypeString(dataType);
        }

        protected override string GetDefaultValue(DbType dataType)
        {
            return DbTypeHelper.GetDefaultValue(dataType);
        }

        protected override void Generate(CreateDatabase op)
        {
            this.AddRun(new CreateDbMigrationRun { Database = op.Database });
        }

        protected override void Generate(DropDatabase op)
        {
            this.AddRun(new DropDbMigrationRun { Database = op.Database });
        }

        protected override void Generate(CreateTable op)
        {
            if (string.IsNullOrWhiteSpace(op.PKName))
            {
                this.AddRun(new GenerationExceptionRun
                {
                    Message = "暂时不支持生成没有主键的表：" + op.TableName
                });
                return;
            }

            using (var sql = this.Writer())
            {
                sql.Write("CREATE TABLE ");
                sql.WriteLine(this.Quote(op.TableName));
                sql.WriteLine("(");
                sql.Indent++;
                this.GenerateColumnDeclaration(sql, op.PKName, op.PKDataType, true);

                //约定，如果是 int 型的主键，默认就是 IDENTITY
                if (op.PKDataType == DbType.Int32) { sql.Write(" IDENTITY(1,1)"); }
                sql.WriteLine();
                sql.Indent--;
                sql.Write(")");

                this.GenerateAddPKConstraint(sql, op.TableName, op.PKName);

                this.AddRun(sql);
            }
        }
    }
}