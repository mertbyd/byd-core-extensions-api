using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Byd.CoreExtensions.Extensions;

/// <summary>
/// Modelin fiziksel veritabani adlarini snake_case bicimine cevirir.
/// </summary>
// islevi: Tablo, view, sema, kolon, anahtar, yabanci anahtar, indeks ve check constraint adlarini model kurulurken cevirir.
// sistemdeki gorevi: Donusum model seviyesinde kaldigi icin EF Core'un migration gecmis tablosu kapsam disinda kalir;
// gecmis tablosunun kolon adlari cerceve varsayilaninda kalir ve modul ile tuketen uygulama ayni deftere yazabilir.
public static class ModelBuilderExtensions
{
    // Modeldeki tum adlari tek geciste cevirir.
    public static void ApplySnakeCaseNaming(this ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var model = builder.Model;
        Rewrite(
            ((IConventionModel)model).GetDefaultSchemaConfigurationSource(),
            () => model.GetDefaultSchema(),
            name => model.SetDefaultSchema(name));

        foreach (var entityType in model.GetEntityTypes())
        {
            // JSON'a eslenen sahipli tipin fiziksel karsiligi bir tablo degil, sahibinin tablosundaki
            // tek bir kolondur. Tablo adi yazilirsa sahiple ayrisir ve model kurulumu durur
            // ("Every entity mapped to JSON must also map to the same table or view as its owner");
            // kolon adi yazilirsa EF bunu JSON ozellik adiyla catisma sayar. Cevrilecek tek ad,
            // JSON belgesini tutan kolonun adidir. EFCore.NamingConventions da ayni ayrimi yapar.
            if (entityType.IsMappedToJson())
            {
                RewriteContainerColumnName(entityType);
                continue;
            }

            RewriteStorageNames(entityType);
            RewriteColumnNames(entityType);
            RewriteConstraintNames(entityType);
        }
    }

    // Entity'nin tablo, view ve sema adlarini cevirir.
    private static void RewriteStorageNames(IMutableEntityType entityType)
    {
        var convention = (IConventionEntityType)entityType;

        Rewrite(convention.GetTableNameConfigurationSource(),
            () => entityType.GetTableName(), name => entityType.SetTableName(name));

        Rewrite(convention.GetSchemaConfigurationSource(),
            () => entityType.GetSchema(), name => entityType.SetSchema(name));

        Rewrite(convention.GetViewNameConfigurationSource(),
            () => entityType.GetViewName(), name => entityType.SetViewName(name));

        Rewrite(convention.GetViewSchemaConfigurationSource(),
            () => entityType.GetViewSchema(), name => entityType.SetViewSchema(name));
    }

    // JSON belgesini tutan kolonun adini cevirir; JSON'a eslenen tipin tek fiziksel adi budur.
    private static void RewriteContainerColumnName(IMutableEntityType entityType)
    {
        Rewrite(((IConventionEntityType)entityType).GetContainerColumnNameConfigurationSource(),
            () => entityType.GetContainerColumnName(),
            name => entityType.SetContainerColumnName(name));
    }

    // Entity'nin kolon adlarini cevirir. ABP ConfigureByConvention audit, tenant ve genisletme kolonlarina property adini
    // elle verir; property adinin aynisi olan ad konvansiyon adi sayilir ve o da cevrilir, ozel verilen ad korunur.
    private static void RewriteColumnNames(IMutableEntityType entityType)
    {
        foreach (var property in entityType.GetProperties())
        {
            var source = property.GetColumnName() == property.Name
                ? ConfigurationSource.Convention
                : ((IConventionProperty)property).GetColumnNameConfigurationSource();
            Rewrite(source, () => property.GetColumnName(), name => property.SetColumnName(name));
        }
    }

    // Entity'nin anahtar, yabanci anahtar, indeks ve check constraint adlarini cevirir.
    private static void RewriteConstraintNames(IMutableEntityType entityType)
    {
        foreach (var key in entityType.GetKeys())
        {
            Rewrite(((IConventionKey)key).GetNameConfigurationSource(),
                () => key.GetName(), name => key.SetName(name));
        }

        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            Rewrite(((IConventionForeignKey)foreignKey).GetConstraintNameConfigurationSource(),
                () => foreignKey.GetConstraintName(), name => foreignKey.SetConstraintName(name));
        }

        foreach (var index in entityType.GetIndexes())
        {
            Rewrite(((IConventionIndex)index).GetDatabaseNameConfigurationSource(),
                () => index.GetDatabaseName(), name => index.SetDatabaseName(name));
        }

        foreach (var checkConstraint in entityType.GetCheckConstraints())
        {
            Rewrite(((IConventionCheckConstraint)checkConstraint).GetNameConfigurationSource(),
                () => checkConstraint.Name, name => checkConstraint.Name = name);
        }
    }

    // Ad konvansiyondan geliyorsa cevirir; elle verilmis ad oldugu gibi korunur.
    private static void Rewrite(ConfigurationSource? source, Func<string?> read, Action<string?> write)
    {
        if (source is null or ConfigurationSource.Convention)
        {
            write(read()?.ConvertToSnakeCase());
        }
    }
}
