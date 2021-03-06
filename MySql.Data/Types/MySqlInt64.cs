// Copyright (c) 2004-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace MySql.Data.Types
{
  internal struct MySqlInt64 : IMySqlValue
  {
    private long mValue;
    private bool isNull;

    public MySqlInt64(bool isNull)
    {
      this.isNull = isNull;
      mValue = 0;
    }

    public MySqlInt64(long val)
    {
      this.isNull = false;
      mValue = val;
    }

    #region IMySqlValue Members

    public bool IsNull
    {
      get { return isNull; }
    }

    MySqlDbType IMySqlValue.MySqlDbType
    {
      get { return MySqlDbType.Int64; }
    }

    DbType IMySqlValue.DbType
    {
      get { return DbType.Int64; }
    }

    object IMySqlValue.Value
    {
      get { return mValue; }
    }

    public long Value
    {
      get { return mValue; }
    }

    Type IMySqlValue.SystemType
    {
      get { return typeof(long); }
    }

    string IMySqlValue.MySqlTypeName
    {
      get { return "BIGINT"; }
    }

    void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
    {
      long v = (val is Int64) ? (Int64)val : Convert.ToInt64(val);
      if (binary)
        packet.WriteInteger(v, 8);
      else
        packet.WriteStringNoNull(v.ToString());
    }

    IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
    {
      if (nullVal)
        return new MySqlInt64(true);

      if (length == -1)
        return new MySqlInt64((long)packet.ReadULong(8));
      else
        return new MySqlInt64(Int64.Parse(packet.ReadString(length)));
    }

    void IMySqlValue.SkipValue(MySqlPacket packet)
    {
      packet.Position += 8;
    }

    #endregion

    internal static void SetDSInfo(DataTable dsTable)
    {
      // we use name indexing because this method will only be called
      // when GetSchema is called for the DataSourceInformation 
      // collection and then it wil be cached.
      DataRow row = dsTable.NewRow();
      row["TypeName"] = "BIGINT";
      row["ProviderDbType"] = MySqlDbType.Int64;
      row["ColumnSize"] = 0;
      row["CreateFormat"] = "BIGINT";
      row["CreateParameters"] = null;
      row["DataType"] = "System.Int64";
      row["IsAutoincrementable"] = true;
      row["IsBestMatch"] = true;
      row["IsCaseSensitive"] = false;
      row["IsFixedLength"] = true;
      row["IsFixedPrecisionScale"] = true;
      row["IsLong"] = false;
      row["IsNullable"] = true;
      row["IsSearchable"] = true;
      row["IsSearchableWithLike"] = false;
      row["IsUnsigned"] = false;
      row["MaximumScale"] = 0;
      row["MinimumScale"] = 0;
      row["IsConcurrencyType"] = DBNull.Value;
      row["IsLiteralSupported"] = false;
      row["LiteralPrefix"] = null;
      row["LiteralSuffix"] = null;
      row["NativeDataType"] = null;
      dsTable.Rows.Add(row);
    }
  }
}
