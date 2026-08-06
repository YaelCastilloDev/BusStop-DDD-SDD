using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusStop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorReactionToBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE comments 
                SET reactions = (
                  SELECT jsonb_agg(
                    jsonb_set(elem - 'ReactionType', '{IsLike}', 
                      CASE WHEN (elem->>'ReactionType')::int = 1 THEN 'true'::jsonb ELSE 'false'::jsonb END
                    )
                  )
                  FROM jsonb_array_elements(reactions) AS elem
                )
                WHERE reactions IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE comments 
                SET reactions = (
                  SELECT jsonb_agg(
                    jsonb_set(elem - 'IsLike', '{ReactionType}', 
                      CASE WHEN (elem->>'IsLike')::boolean THEN '1'::jsonb ELSE '0'::jsonb END
                    )
                  )
                  FROM jsonb_array_elements(reactions) AS elem
                )
                WHERE reactions IS NOT NULL;
            ");
        }
    }
}
