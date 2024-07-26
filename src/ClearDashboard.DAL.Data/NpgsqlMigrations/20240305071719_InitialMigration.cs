using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.NpgsqlMigrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "label_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_label_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "labels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    template_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_labels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    last_alignment_level_id = table.Column<int>(type: "integer", nullable: true),
                    default_label_group_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "label_group_associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label_group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_label_group_associations", x => x.id);
                    table.ForeignKey(
                        name: "fk_label_group_associations_label_groups_label_group_id",
                        column: x => x.label_group_id,
                        principalTable: "label_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_label_group_associations_labels_label_id",
                        column: x => x.label_id,
                        principalTable: "labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "corpa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_rtl = table.Column<bool>(type: "boolean", nullable: false),
                    font_family = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "text", nullable: true),
                    paratext_guid = table.Column<string>(type: "text", nullable: true),
                    corpus_type = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_corpa", x => x.id);
                    table.ForeignKey(
                        name: "fk_corpa_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "corpa_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_rtl = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "text", nullable: true),
                    paratext_guid = table.Column<string>(type: "text", nullable: true),
                    corpus_type = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_corpa_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_corpa_history_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lexicon_lexemes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "text", nullable: true),
                    lemma = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lexicon_lexemes", x => x.id);
                    table.ForeignKey(
                        name: "fk_lexicon_lexemes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lexicon_semantic_domains",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lexicon_semantic_domains", x => x.id);
                    table.ForeignKey(
                        name: "fk_lexicon_semantic_domains_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    abbreviated_text = table.Column<string>(type: "text", nullable: true),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note_status = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_notes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_name = table.Column<string>(type: "text", nullable: true),
                    is_rtl = table.Column<bool>(type: "boolean", nullable: false),
                    design_surface_layout = table.Column<string>(type: "text", nullable: true),
                    window_tab_layout = table.Column<string>(type: "text", nullable: true),
                    app_version = table.Column<string>(type: "text", nullable: true),
                    last_merged_commit_sha = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokenized_corpora",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    corpus_history_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    tokenization_function = table.Column<string>(type: "text", nullable: true),
                    scr_vers_type = table.Column<int>(type: "integer", nullable: false),
                    custom_vers_data = table.Column<string>(type: "text", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    last_tokenized = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokenized_corpora", x => x.id);
                    table.ForeignKey(
                        name: "fk_tokenized_corpora_corpa_corpus_id",
                        column: x => x.corpus_id,
                        principalTable: "corpa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tokenized_corpora_corpa_history_corpus_history_id",
                        column: x => x.corpus_history_id,
                        principalTable: "corpa_history",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_tokenized_corpora_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lexicon_forms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    lexeme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lexicon_forms", x => x.id);
                    table.ForeignKey(
                        name: "fk_lexicon_forms_lexicon_lexemes_lexeme_id",
                        column: x => x.lexeme_id,
                        principalTable: "lexicon_lexemes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lexicon_forms_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lexicon_meanings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "text", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    lexeme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lexicon_meanings", x => x.id);
                    table.ForeignKey(
                        name: "fk_lexicon_meanings_lexicon_lexemes_lexeme_id",
                        column: x => x.lexeme_id,
                        principalTable: "lexicon_lexemes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lexicon_meanings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "label_note_associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_label_note_associations", x => x.id);
                    table.ForeignKey(
                        name: "fk_label_note_associations_labels_label_id",
                        column: x => x.label_id,
                        principalTable: "labels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_label_note_associations_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "note_domain_entity_associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain_entity_id_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    domain_entity_id_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note_domain_entity_associations", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_domain_entity_associations_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "note_recipient",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_type = table.Column<int>(type: "integer", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note_recipient", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_recipient_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_note_recipient_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "note_user_seen_associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note_user_seen_associations", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_user_seen_associations_notes_note_id",
                        column: x => x.note_id,
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_note_user_seen_associations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parallel_corpa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    source_tokenized_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_tokenized_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parallel_corpa", x => x.id);
                    table.ForeignKey(
                        name: "fk_parallel_corpa_tokenized_corpora_source_tokenized_corpus_id",
                        column: x => x.source_tokenized_corpus_id,
                        principalTable: "tokenized_corpora",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parallel_corpa_tokenized_corpora_target_tokenized_corpus_id",
                        column: x => x.target_tokenized_corpus_id,
                        principalTable: "tokenized_corpora",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parallel_corpa_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parallel_corpa_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    source_tokenized_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_tokenized_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parallel_corpa_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_parallel_corpa_history_tokenized_corpora_source_tokenized_c",
                        column: x => x.source_tokenized_corpus_id,
                        principalTable: "tokenized_corpora",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parallel_corpa_history_tokenized_corpora_target_tokenized_c",
                        column: x => x.target_tokenized_corpus_id,
                        principalTable: "tokenized_corpora",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parallel_corpa_history_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verse_rows",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    book_chapter_verse = table.Column<string>(type: "text", nullable: true),
                    original_text = table.Column<string>(type: "text", nullable: true),
                    is_sentence_start = table.Column<bool>(type: "boolean", nullable: false),
                    is_in_range = table.Column<bool>(type: "boolean", nullable: false),
                    is_range_start = table.Column<bool>(type: "boolean", nullable: false),
                    is_empty = table.Column<bool>(type: "boolean", nullable: false),
                    tokenized_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verse_rows", x => x.id);
                    table.ForeignKey(
                        name: "fk_verse_rows_tokenized_corpora_tokenized_corpus_id",
                        column: x => x.tokenized_corpus_id,
                        principalTable: "tokenized_corpora",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verse_rows_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lexicon_semantic_domain_meaning_associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    semantic_domain_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meaning_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lexicon_semantic_domain_meaning_associations", x => x.id);
                    table.ForeignKey(
                        name: "fk_lexicon_semantic_domain_meaning_associations_lexicon_meanin",
                        column: x => x.meaning_id,
                        principalTable: "lexicon_meanings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lexicon_semantic_domain_meaning_associations_lexicon_semant",
                        column: x => x.semantic_domain_id,
                        principalTable: "lexicon_semantic_domains",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lexicon_translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    originated_from = table.Column<string>(type: "jsonb", nullable: true),
                    meaning_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lexicon_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_lexicon_translations_lexicon_meanings_meaning_id",
                        column: x => x.meaning_id,
                        principalTable: "lexicon_meanings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lexicon_translations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alignment_sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parallel_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    smt_model = table.Column<string>(type: "text", nullable: true),
                    is_syntax_tree_aligner_refined = table.Column<bool>(type: "boolean", nullable: false),
                    is_symmetrized = table.Column<bool>(type: "boolean", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    parallel_corpus_history_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alignment_sets", x => x.id);
                    table.ForeignKey(
                        name: "fk_alignment_sets_parallel_corpa_history_parallel_corpus_histo",
                        column: x => x.parallel_corpus_history_id,
                        principalTable: "parallel_corpa_history",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_alignment_sets_parallel_corpa_parallel_corpus_id",
                        column: x => x.parallel_corpus_id,
                        principalTable: "parallel_corpa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alignment_sets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verse_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parallel_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parallel_corpus_history_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verse_mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_verse_mappings_parallel_corpa_history_parallel_corpus_histo",
                        column: x => x.parallel_corpus_history_id,
                        principalTable: "parallel_corpa_history",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verse_mappings_parallel_corpa_parallel_corpus_id",
                        column: x => x.parallel_corpus_id,
                        principalTable: "parallel_corpa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verse_mappings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "token_components",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    engine_token_id = table.Column<string>(type: "text", nullable: true),
                    training_text = table.Column<string>(type: "text", nullable: true),
                    surface_text = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    extended_properties = table.Column<string>(type: "text", nullable: true),
                    verse_row_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tokenized_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    book_number = table.Column<int>(type: "integer", nullable: true),
                    chapter_number = table.Column<int>(type: "integer", nullable: true),
                    verse_number = table.Column<int>(type: "integer", nullable: true),
                    word_number = table.Column<int>(type: "integer", nullable: true),
                    subword_number = table.Column<int>(type: "integer", nullable: true),
                    origin_token_location = table.Column<string>(type: "text", nullable: true),
                    parallel_corpus_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_token_components", x => x.id);
                    table.ForeignKey(
                        name: "fk_token_components_parallel_corpa_parallel_corpus_id",
                        column: x => x.parallel_corpus_id,
                        principalTable: "parallel_corpa",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_token_components_tokenized_corpora_tokenized_corpus_id",
                        column: x => x.tokenized_corpus_id,
                        principalTable: "tokenized_corpora",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_token_components_verse_rows_verse_row_id",
                        column: x => x.verse_row_id,
                        principalTable: "verse_rows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alignment_set_denormalization_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alignment_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alignment_set_denormalization_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_alignment_set_denormalization_tasks_alignment_sets_alignmen",
                        column: x => x.alignment_set_id,
                        principalTable: "alignment_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translation_sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    derived_from_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parallel_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alignment_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    parallel_corpus_history_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translation_sets", x => x.id);
                    table.ForeignKey(
                        name: "fk_translation_sets_alignment_sets_alignment_set_id",
                        column: x => x.alignment_set_id,
                        principalTable: "alignment_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_translation_sets_parallel_corpa_history_parallel_corpus_his",
                        column: x => x.parallel_corpus_history_id,
                        principalTable: "parallel_corpa_history",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_translation_sets_parallel_corpa_parallel_corpus_id",
                        column: x => x.parallel_corpus_id,
                        principalTable: "parallel_corpa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_translation_sets_translation_sets_derived_from_id",
                        column: x => x.derived_from_id,
                        principalTable: "translation_sets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_translation_sets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    verse_number = table.Column<int>(type: "integer", nullable: true),
                    book_number = table.Column<int>(type: "integer", nullable: true),
                    chapter_number = table.Column<int>(type: "integer", nullable: true),
                    verse_text = table.Column<string>(type: "text", nullable: true),
                    corpus_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parallel_corpus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verse_mapping_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bbbcccvvv = table.Column<string>(type: "text", nullable: true),
                    corpus_history_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verses", x => x.id);
                    table.ForeignKey(
                        name: "fk_verses_corpa_corpus_id",
                        column: x => x.corpus_id,
                        principalTable: "corpa",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verses_corpa_history_corpus_history_id",
                        column: x => x.corpus_history_id,
                        principalTable: "corpa_history",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verses_parallel_corpa_parallel_corpus_id",
                        column: x => x.parallel_corpus_id,
                        principalTable: "parallel_corpa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verses_verse_mappings_verse_mapping_id",
                        column: x => x.verse_mapping_id,
                        principalTable: "verse_mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "adornments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lemma = table.Column<string>(type: "text", nullable: true),
                    parts_of_speech = table.Column<string>(type: "text", nullable: true),
                    strong = table.Column<string>(type: "text", nullable: true),
                    token_morphology = table.Column<string>(type: "text", nullable: true),
                    token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adornments", x => x.id);
                    table.ForeignKey(
                        name: "fk_adornments_tokens_token_id",
                        column: x => x.token_id,
                        principalTable: "token_components",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_adornments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alignment_top_target_training_texts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alignment_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_token_component_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_training_text = table.Column<string>(type: "text", nullable: false),
                    top_target_training_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alignment_top_target_training_texts", x => x.id);
                    table.ForeignKey(
                        name: "fk_alignment_top_target_training_texts_alignment_sets_alignmen",
                        column: x => x.alignment_set_id,
                        principalTable: "alignment_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alignment_top_target_training_texts_token_components_source",
                        column: x => x.source_token_component_id,
                        principalTable: "token_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_token_component_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_token_component_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alignment_verification = table.Column<int>(type: "integer", nullable: false),
                    alignment_originated_from = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<double>(type: "double precision", nullable: false),
                    alignment_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_alignments_alignment_sets_alignment_set_id",
                        column: x => x.alignment_set_id,
                        principalTable: "alignment_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alignments_token_components_source_token_component_id",
                        column: x => x.source_token_component_id,
                        principalTable: "token_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alignments_token_components_target_token_component_id",
                        column: x => x.target_token_component_id,
                        principalTable: "token_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_alignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "token_composite_token_associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_composite_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_token_composite_token_associations", x => x.id);
                    table.ForeignKey(
                        name: "fk_token_composite_token_associations_token_composites_token_c",
                        column: x => x.token_composite_id,
                        principalTable: "token_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_token_composite_token_associations_tokens_token_id",
                        column: x => x.token_id,
                        principalTable: "token_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translation_model_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    translation_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translation_model_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_translation_model_entries_translation_sets_translation_set_",
                        column: x => x.translation_set_id,
                        principalTable: "translation_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_token_component_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_text = table.Column<string>(type: "text", nullable: true),
                    translation_state = table.Column<int>(type: "integer", nullable: false),
                    translation_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lexicon_translation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_translations_lexicon_translations_lexicon_translation_id",
                        column: x => x.lexicon_translation_id,
                        principalTable: "lexicon_translations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_translations_token_components_source_token_component_id",
                        column: x => x.source_token_component_id,
                        principalTable: "token_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_translations_translation_sets_translation_set_id",
                        column: x => x.translation_set_id,
                        principalTable: "translation_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_translations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "token_verse_associations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_component_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    verse_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_token_verse_associations", x => x.id);
                    table.ForeignKey(
                        name: "fk_token_verse_associations_token_components_token_component_id",
                        column: x => x.token_component_id,
                        principalTable: "token_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_token_verse_associations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_token_verse_associations_verses_verse_id",
                        column: x => x.verse_id,
                        principalTable: "verses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translation_model_target_text_score",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    translation_model_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    score = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translation_model_target_text_score", x => x.id);
                    table.ForeignKey(
                        name: "fk_translation_model_target_text_score_translation_model_entri",
                        column: x => x.translation_model_entry_id,
                        principalTable: "translation_model_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_adornments_token_id",
                table: "adornments",
                column: "token_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_adornments_user_id",
                table: "adornments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignment_set_denormalization_tasks_alignment_set_id",
                table: "alignment_set_denormalization_tasks",
                column: "alignment_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignment_sets_parallel_corpus_history_id",
                table: "alignment_sets",
                column: "parallel_corpus_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignment_sets_parallel_corpus_id",
                table: "alignment_sets",
                column: "parallel_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignment_sets_user_id",
                table: "alignment_sets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Align_Top_Training_Align_Set_Id_Source_Token_Id",
                table: "alignment_top_target_training_texts",
                columns: new[] { "alignment_set_id", "source_token_component_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Align_Top_Training_Align_Set_Id_Source_Training",
                table: "alignment_top_target_training_texts",
                columns: new[] { "alignment_set_id", "source_training_text" });

            migrationBuilder.CreateIndex(
                name: "IX_Align_Top_Training_Source_Token_Id",
                table: "alignment_top_target_training_texts",
                column: "source_token_component_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignments_alignment_set_id",
                table: "alignments",
                column: "alignment_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignments_source_token_component_id",
                table: "alignments",
                column: "source_token_component_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignments_target_token_component_id",
                table: "alignments",
                column: "target_token_component_id");

            migrationBuilder.CreateIndex(
                name: "ix_alignments_user_id",
                table: "alignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_corpa_user_id",
                table: "corpa",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_corpa_history_user_id",
                table: "corpa_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_label_group_associations_label_group_id",
                table: "label_group_associations",
                column: "label_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_label_group_associations_label_id",
                table: "label_group_associations",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "ix_label_groups_name",
                table: "label_groups",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_label_note_associations_label_id",
                table: "label_note_associations",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "ix_label_note_associations_note_id",
                table: "label_note_associations",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_labels_text",
                table: "labels",
                column: "text",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_forms_lexeme_id",
                table: "lexicon_forms",
                column: "lexeme_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_forms_text",
                table: "lexicon_forms",
                column: "text");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_forms_user_id",
                table: "lexicon_forms",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Lexeme_Lemma_TypeNotNull_Language",
                table: "lexicon_lexemes",
                columns: new[] { "lemma", "type", "language" },
                unique: true,
                filter: "Type IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lexicon_Lexeme_Lemma_TypeNull_Language",
                table: "lexicon_lexemes",
                columns: new[] { "lemma", "language" },
                unique: true,
                filter: "Type IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_lexemes_user_id",
                table: "lexicon_lexemes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_meanings_lexeme_id",
                table: "lexicon_meanings",
                column: "lexeme_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_meanings_user_id",
                table: "lexicon_meanings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_semantic_domain_meaning_associations_meaning_id",
                table: "lexicon_semantic_domain_meaning_associations",
                column: "meaning_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_semantic_domain_meaning_associations_semantic_domai",
                table: "lexicon_semantic_domain_meaning_associations",
                column: "semantic_domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_semantic_domains_user_id",
                table: "lexicon_semantic_domains",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_translations_meaning_id",
                table: "lexicon_translations",
                column: "meaning_id");

            migrationBuilder.CreateIndex(
                name: "ix_lexicon_translations_user_id",
                table: "lexicon_translations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_domain_entity_associations_note_id",
                table: "note_domain_entity_associations",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_recipient_note_id",
                table: "note_recipient",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_recipient_user_id",
                table: "note_recipient",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_user_seen_associations_note_id",
                table: "note_user_seen_associations",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_user_seen_associations_user_id",
                table: "note_user_seen_associations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_user_id",
                table: "notes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_parallel_corpa_source_tokenized_corpus_id",
                table: "parallel_corpa",
                column: "source_tokenized_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_parallel_corpa_target_tokenized_corpus_id",
                table: "parallel_corpa",
                column: "target_tokenized_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_parallel_corpa_user_id",
                table: "parallel_corpa",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_parallel_corpa_history_source_tokenized_corpus_id",
                table: "parallel_corpa_history",
                column: "source_tokenized_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_parallel_corpa_history_target_tokenized_corpus_id",
                table: "parallel_corpa_history",
                column: "target_tokenized_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_parallel_corpa_history_user_id",
                table: "parallel_corpa_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_user_id",
                table: "projects",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_book_number",
                table: "token_components",
                column: "book_number");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_chapter_number",
                table: "token_components",
                column: "chapter_number");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_engine_token_id",
                table: "token_components",
                column: "engine_token_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_origin_token_location",
                table: "token_components",
                column: "origin_token_location");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_parallel_corpus_id",
                table: "token_components",
                column: "parallel_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_surface_text",
                table: "token_components",
                column: "surface_text");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_tokenized_corpus_id",
                table: "token_components",
                column: "tokenized_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_training_text",
                table: "token_components",
                column: "training_text");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_verse_number",
                table: "token_components",
                column: "verse_number");

            migrationBuilder.CreateIndex(
                name: "ix_token_components_verse_row_id",
                table: "token_components",
                column: "verse_row_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_composite_token_associations_token_composite_id",
                table: "token_composite_token_associations",
                column: "token_composite_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_composite_token_associations_token_id",
                table: "token_composite_token_associations",
                column: "token_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_verse_associations_token_component_id",
                table: "token_verse_associations",
                column: "token_component_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_verse_associations_user_id",
                table: "token_verse_associations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_verse_associations_verse_id",
                table: "token_verse_associations",
                column: "verse_id");

            migrationBuilder.CreateIndex(
                name: "ix_tokenized_corpora_corpus_history_id",
                table: "tokenized_corpora",
                column: "corpus_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_tokenized_corpora_corpus_id",
                table: "tokenized_corpora",
                column: "corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_tokenized_corpora_user_id",
                table: "tokenized_corpora",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Trans_Model_Entry_Trans_Set_Id_Source_Text",
                table: "translation_model_entries",
                columns: new[] { "translation_set_id", "source_text" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trans_Model_Score_Entry_Id_Text",
                table: "translation_model_target_text_score",
                columns: new[] { "translation_model_entry_id", "text" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_translation_sets_alignment_set_id",
                table: "translation_sets",
                column: "alignment_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_translation_sets_derived_from_id",
                table: "translation_sets",
                column: "derived_from_id");

            migrationBuilder.CreateIndex(
                name: "ix_translation_sets_parallel_corpus_history_id",
                table: "translation_sets",
                column: "parallel_corpus_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_translation_sets_parallel_corpus_id",
                table: "translation_sets",
                column: "parallel_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_translation_sets_user_id",
                table: "translation_sets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_translations_lexicon_translation_id",
                table: "translations",
                column: "lexicon_translation_id");

            migrationBuilder.CreateIndex(
                name: "ix_translations_source_token_component_id",
                table: "translations",
                column: "source_token_component_id");

            migrationBuilder.CreateIndex(
                name: "ix_translations_translation_set_id",
                table: "translations",
                column: "translation_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_translations_user_id",
                table: "translations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_verse_mappings_parallel_corpus_history_id",
                table: "verse_mappings",
                column: "parallel_corpus_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_verse_mappings_parallel_corpus_id",
                table: "verse_mappings",
                column: "parallel_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_verse_mappings_user_id",
                table: "verse_mappings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_verse_rows_book_chapter_verse",
                table: "verse_rows",
                column: "book_chapter_verse");

            migrationBuilder.CreateIndex(
                name: "ix_verse_rows_tokenized_corpus_id",
                table: "verse_rows",
                column: "tokenized_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_verse_rows_user_id",
                table: "verse_rows",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_verses_bbbcccvvv",
                table: "verses",
                column: "bbbcccvvv");

            migrationBuilder.CreateIndex(
                name: "ix_verses_book_number",
                table: "verses",
                column: "book_number");

            migrationBuilder.CreateIndex(
                name: "ix_verses_chapter_number",
                table: "verses",
                column: "chapter_number");

            migrationBuilder.CreateIndex(
                name: "ix_verses_corpus_history_id",
                table: "verses",
                column: "corpus_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_verses_corpus_id",
                table: "verses",
                column: "corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_verses_parallel_corpus_id",
                table: "verses",
                column: "parallel_corpus_id");

            migrationBuilder.CreateIndex(
                name: "ix_verses_user_id",
                table: "verses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_verses_verse_mapping_id",
                table: "verses",
                column: "verse_mapping_id");

            migrationBuilder.CreateIndex(
                name: "ix_verses_verse_number",
                table: "verses",
                column: "verse_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adornments");

            migrationBuilder.DropTable(
                name: "alignment_set_denormalization_tasks");

            migrationBuilder.DropTable(
                name: "alignment_top_target_training_texts");

            migrationBuilder.DropTable(
                name: "alignments");

            migrationBuilder.DropTable(
                name: "label_group_associations");

            migrationBuilder.DropTable(
                name: "label_note_associations");

            migrationBuilder.DropTable(
                name: "lexicon_forms");

            migrationBuilder.DropTable(
                name: "lexicon_semantic_domain_meaning_associations");

            migrationBuilder.DropTable(
                name: "note_domain_entity_associations");

            migrationBuilder.DropTable(
                name: "note_recipient");

            migrationBuilder.DropTable(
                name: "note_user_seen_associations");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "token_composite_token_associations");

            migrationBuilder.DropTable(
                name: "token_verse_associations");

            migrationBuilder.DropTable(
                name: "translation_model_target_text_score");

            migrationBuilder.DropTable(
                name: "translations");

            migrationBuilder.DropTable(
                name: "label_groups");

            migrationBuilder.DropTable(
                name: "labels");

            migrationBuilder.DropTable(
                name: "lexicon_semantic_domains");

            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "verses");

            migrationBuilder.DropTable(
                name: "translation_model_entries");

            migrationBuilder.DropTable(
                name: "lexicon_translations");

            migrationBuilder.DropTable(
                name: "token_components");

            migrationBuilder.DropTable(
                name: "verse_mappings");

            migrationBuilder.DropTable(
                name: "translation_sets");

            migrationBuilder.DropTable(
                name: "lexicon_meanings");

            migrationBuilder.DropTable(
                name: "verse_rows");

            migrationBuilder.DropTable(
                name: "alignment_sets");

            migrationBuilder.DropTable(
                name: "lexicon_lexemes");

            migrationBuilder.DropTable(
                name: "parallel_corpa_history");

            migrationBuilder.DropTable(
                name: "parallel_corpa");

            migrationBuilder.DropTable(
                name: "tokenized_corpora");

            migrationBuilder.DropTable(
                name: "corpa");

            migrationBuilder.DropTable(
                name: "corpa_history");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
